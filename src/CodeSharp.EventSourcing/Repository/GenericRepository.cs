//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 一个泛型仓储，提供基本的Add以及GetById这两个方法来管理特定类型的聚合根
    /// </summary>
    internal class GenericRepository<T> where T : AggregateRoot
    {
        #region Private Variables

        private IEventStoreProvider _eventStoreProvider;
        private ISnapshotStoreProvider _snapshotStoreProvider;
        private IAggregateRootFactory _aggregateRootFactory;
        private IUnitOfWorkManager _unitOfWorkManager;

        #endregion

        #region Constructors

        public GenericRepository() : this(
            DependencyResolver.Resolve<IEventStoreProvider>(),
            DependencyResolver.Resolve<ISnapshotStoreProvider>(),
            DependencyResolver.Resolve<IAggregateRootFactory>(),
            DependencyResolver.Resolve<IUnitOfWorkManager>())
        {
        }
        public GenericRepository(
            IEventStoreProvider eventStoreProvider,
            ISnapshotStoreProvider snapshotStoreProvider,
            IAggregateRootFactory aggregateRootFactory,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _eventStoreProvider = eventStoreProvider;
            _snapshotStoreProvider = snapshotStoreProvider;
            _aggregateRootFactory = aggregateRootFactory;
            _unitOfWorkManager = unitOfWorkManager;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 将一个聚合根添加到仓储中，默认的实现是将其注册到一个当前可用的UnitOfWork，仓储本身不保存该聚合根；
        /// 如果当前添加的聚合根为空或者当前上下文不存在一个UnitOfWork，则抛出相应异常
        /// </summary>
        public virtual void Add(T aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }

            AssertUnitOfWorkExisting();
            TrackingAggregateRoot(aggregateRoot);
        }
        /// <summary>
        /// 返回一个聚合根，不锁住该聚合根，所以可能出现多个人同时获取到同一个聚合根的情况，
        /// 如果当前业务要求你的读的时候别人不允许再读，则需要使用GetByIdWithLock方法
        /// </summary>
        public virtual T GetById(string aggregateRootId)
        {
            //尝试从当前上下文的UnitOfWork中获取聚合根
            T aggregateRoot = GetFromUnitOfWork(aggregateRootId);
            if (aggregateRoot != null)
            {
                return aggregateRoot;
            }

            //从持久化设备中获取聚合根
            aggregateRoot = GetFromStorage(aggregateRootId);

            //如果聚合根不为空，则通知UnitOfWork跟踪获取到的聚合根
            if (aggregateRoot != null)
            {
                TrackingAggregateRoot(aggregateRoot);
            }

            //最后返回聚合根
            return aggregateRoot;
        }
        /// <summary>
        /// 返回一个聚合根，返回之前会现在数据库级别锁住该聚合根，锁的级别是行级排它锁(ROWLOCK,XLOCK)，
        /// 确保你读取之后，事务执行完成之前，别人都不允许读取该聚合根
        /// </summary>
        public virtual T GetByIdWithLock(string aggregateRootId)
        {
            LockAggregateRoot(aggregateRootId);
            return GetById(aggregateRootId);
        }
        /// <summary>
        /// 根据给定的事件批量获取聚合根，通过重演事件方式重新得到聚合根，并将聚合根添加到UnitOfWork进行管理
        /// </summary>
        public IList<T> GetFromEvents<TAggregateRootEvent>(IEnumerable<TAggregateRootEvent> evnts) where TAggregateRootEvent : AggregateRootEvent
        {
            var aggregateRootList = new List<T>();
            var aggregateRootType = typeof(T);
            var evntGroups = evnts.GroupBy(x => x.AggregateRootId).Where(x => x.Count() > 0);
            var unitOfWork = _unitOfWorkManager.GetUnitOfWork();

            foreach (var evntGroup in evntGroups)
            {
                var aggregateRoot = DependencyResolver.Resolve<IAggregateRootFactory>().CreateAggregateRoot(aggregateRootType) as T;
                aggregateRoot.ReplayEvents(evntGroup);
                unitOfWork.TrackingAggregateRoot(aggregateRoot);
                aggregateRootList.Add(aggregateRoot);
            }

            return aggregateRootList;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 从当前的UnitOfWork中查找一个聚合根
        /// </summary>
        private T GetFromUnitOfWork(string aggregateRootId)
        {
            var unitofWork = _unitOfWorkManager.GetUnitOfWork();
            if (unitofWork != null)
            {
                var trackingAggregateRoots = unitofWork.GetAllTrackingAggregateRoots();
                if (trackingAggregateRoots != null && trackingAggregateRoots.Count() > 0)
                {
                    return trackingAggregateRoots.FirstOrDefault(
                        trackingAggregateRoot => trackingAggregateRoot.GetType() == typeof(T)
                            && trackingAggregateRoot.UniqueId == aggregateRootId) as T;
                }
            }
            return null;
        }
        /// <summary>
        /// 从存储设备中查找一个聚合根
        /// </summary>
        private T GetFromStorage(string aggregateRootId)
        {
            T aggregateRoot = null;
            long maxEventVersion = long.MaxValue;
            long minEventVersion = long.MinValue;

            //尝试从快照获取聚合根
            bool success = TryGetFromSnapshot(aggregateRootId, maxEventVersion, out aggregateRoot);

            //如果从快照获取不成功，则尝试从第一个事件开始重演所有事件从而得到聚合根
            if (!success)
            {
                var allCommittedEvents = _eventStoreProvider.GetEventStore<T>().GetEvents(aggregateRootId, minEventVersion, maxEventVersion);
                aggregateRoot = BuildAggregateRootFromCommittedEvents(allCommittedEvents);
            }

            //最后返回聚合根
            return aggregateRoot;
        }
        /// <summary>
        /// 通过事件溯源的方式重建聚合根
        /// </summary>
        private T BuildAggregateRootFromCommittedEvents(IEnumerable<AggregateRootEvent> evnts)
        {
            T aggregateRoot = null;

            if (evnts != null && evnts.Count() > 0)
            {
                aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(typeof(T)) as T;
                aggregateRoot.ReplayEvents(evnts);
            }

            return aggregateRoot;
        }
        /// <summary>
        /// 锁住给定的聚合根
        /// </summary>
        private void LockAggregateRoot(string aggregateRootId)
        {
            _eventStoreProvider.GetEventStore<T>().LockAggregateRoot(aggregateRootId);
        }
        /// <summary>
        /// 通知UnitOfWork跟踪给定的聚合根
        /// </summary>
        private void TrackingAggregateRoot(T aggregateRoot)
        {
            _unitOfWorkManager.GetUnitOfWork().TrackingAggregateRoot(aggregateRoot);
        }
        /// <summary>
        /// 判断当前是否存在一个有效的UnitOfWork实例，如果不存在，则抛出异常
        /// </summary>
        private void AssertUnitOfWorkExisting()
        {
            if (_unitOfWorkManager.GetUnitOfWork() == null)
            {
                throw new EventSourcingException("当前不存在一个可用的UnitOfWork");
            }
        }
        /// <summary>
        /// 尝试从快照获取聚合根
        /// </summary>
        private bool TryGetFromSnapshot(string aggregateRootId, long maxEventVersion, out T aggregateRoot)
        {
            aggregateRoot = null;

            var snapshot = _snapshotStoreProvider.GetSnapshotStore<T>().GetLastSnapshot(aggregateRootId, maxEventVersion);
            if (snapshot != null && snapshot.IsValid())
            {
                T aggregateRootFromSnapshot = DependencyResolver.Resolve<ISnapshotter>().RestoreFromSnapshot(snapshot) as T;
                if (aggregateRootFromSnapshot != null)
                {
                    //验证从快照还原出来的聚合根是否有效
                    ValidateAggregateRootFromSnapshot(aggregateRootFromSnapshot, aggregateRootId);
                    //如果从快照得到的聚合根有效，则将发生在该快照之后的事件进行重演
                    var committedEventsAfterSnapshot = _eventStoreProvider.GetEventStore<T>().GetEvents(aggregateRootId, snapshot.Version + 1, maxEventVersion);
                    aggregateRootFromSnapshot.ReplayEvents(committedEventsAfterSnapshot);
                    aggregateRoot = aggregateRootFromSnapshot;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 验证从快照获取到的聚合根的合法性
        /// </summary>
        private void ValidateAggregateRootFromSnapshot(T aggregateRootFromSnapshot, string requiredAggregateRootId)
        {
            if (aggregateRootFromSnapshot.UniqueId != requiredAggregateRootId)
            {
                string message = string.Format("从快照还原出来的聚合根的Id({0})与所要求的Id({1})不符",
                    aggregateRootFromSnapshot.UniqueId,
                    requiredAggregateRootId);
                throw new EventSourcingException(message);
            }
        }

        #endregion
    }
}
