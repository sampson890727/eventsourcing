//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;

namespace CodeSharp.EventSourcing.NHibernate
{
    /// <summary>
    /// IEventStore接口的基于NHiberate实现的基类
    /// </summary>
    [Transactional]
    public class NHibernateEventStoreBase<T> : IEventStore<T> where T : AggregateRoot
    {
        #region Private Variables

        private INHibernateSessionManager _sessionManager;
        private IJsonSerializer _eventSerializer;
        private ITypeNameMapper _typeNameMapper;
        private IAggregateRootEventTypeProvider _aggregateRootEventTypeProvider;
        private ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public NHibernateEventStoreBase()
            : this(
            DependencyResolver.Resolve<INHibernateSessionManager>(),
            DependencyResolver.Resolve<IJsonSerializer>(),
            DependencyResolver.Resolve<ITypeNameMapper>(),
            DependencyResolver.Resolve<IAggregateRootEventTypeProvider>(),
            DependencyResolver.Resolve<ILoggerFactory>())
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public NHibernateEventStoreBase(
            INHibernateSessionManager sessionManager,
            IJsonSerializer eventSerializer,
            ITypeNameMapper typeNameMapper,
            IAggregateRootEventTypeProvider aggregateRootEventTypeProvider,
            ILoggerFactory loggerFactory)
        {
            _sessionManager = sessionManager;
            _eventSerializer = eventSerializer;
            _typeNameMapper = typeNameMapper;
            _aggregateRootEventTypeProvider = aggregateRootEventTypeProvider;
            _logger = loggerFactory.Create("EventSourcing.NHibernateEventStoreBase");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取指定聚合根的指定版本号范围内的按升序排序的事件
        /// </summary>
        [Transaction]
        public virtual IEnumerable<AggregateRootEvent> GetEvents(string aggregateRootId, long minVersion, long maxVersion)
        {
            List<AggregateRootEvent> evnts = new List<AggregateRootEvent>();

            //组装查询Criteria
            Type eventType = _aggregateRootEventTypeProvider.GetAggregateRootEventType(typeof(T));
            ICriteria criteria = OpenSession().CreateCriteria(eventType);
            criteria.Add(Restrictions.Eq("AggregateRootId", aggregateRootId));
            criteria.Add(Restrictions.Eq("AggregateRootName", GetAggregateRootName()));
            criteria.Add(Restrictions.Ge("Version", minVersion));
            criteria.Add(Restrictions.Le("Version", maxVersion));

            //执行查询操作
            var eventList = typeof(EventQueryHelper).GetMethod("GetList").MakeGenericMethod(eventType).Invoke(new EventQueryHelper(), new object[] { criteria }) as IList;

            //转换查询结果
            foreach (var evnt in eventList)
            {
                var aggregateRootEvent = evnt as AggregateRootEvent;
                aggregateRootEvent.AggregateRootType = GetAggregateRootType(aggregateRootEvent.AggregateRootName);
                aggregateRootEvent.Event = DeserializeEvent(GetEventType(aggregateRootEvent.Name), aggregateRootEvent.Data);
                evnts.Add(aggregateRootEvent);
            }

            return evnts;
        }
        /// <summary>
        /// 将给定的事件持久化到数据库，利用NHibernate实现持久化；
        /// 在方法内部，框架会确保整个操作在事务内完成；
        /// </summary>
        [Transaction]
        public virtual void StoreEvents(IEnumerable<AggregateRootEvent> evnts)
        {
            //如果需要处理的事件个数为0，则直接退出
            if (evnts.Count() == 0)
            {
                return;
            }

            //验证事件的数据有效性
            EventSourcingHelper.AreEventsBelongtoSameAggregateRoot<T>(evnts);

            try
            {
                //持久化事件之前先Lock聚合根
                var version = LockAggregateRoot(evnts.First().AggregateRootId);
                //持久化当前聚合根的事件
                PersistAggregateRootEvents(evnts, version);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("持久化聚合根的事件时出现异常，被持久化的事件:{0}，异常详细信息：{1}",
                    string.Join("|", evnts.Select(x => string.Format("【{0}】", x.ToString())).ToArray()),
                    ex);
                throw;
            }
        }
        /// <summary>
        /// Lock一个给定的聚合根，数据库级别的锁
        /// </summary>
        [Transaction]
        public long LockAggregateRoot(string aggregateRootId)
        {
            return LockAggregateRootAfterTransactionStarted(aggregateRootId);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 开启一个NHibernate Session
        /// </summary>
        protected ISession OpenSession()
        {
            return _sessionManager.OpenSession();
        }
        /// <summary>
        /// 持久化聚合根的给定的事件，该方法内部不需要考虑事务或并发问题，只要实现持久化逻辑即可，
        /// 事务和锁的问题框架会负责处理；
        /// </summary>
        protected virtual void PersistAggregateRootEvents(IEnumerable<AggregateRootEvent> evnts, long baseVersion)
        {
            //向数据库循环插入当前聚合根的所有事件
            long currentIndex = 1;
            foreach (var evnt in evnts)
            {
                PersistNewEvent(evnt, baseVersion + currentIndex);
                currentIndex++;
            }
        }
        /// <summary>
        /// 向数据库中插入一个新的事件
        /// </summary>
        protected virtual void PersistNewEvent(AggregateRootEvent evnt, long version)
        {
            evnt.Version = version;
            evnt.AggregateRootName = GetAggregateRootName();
            evnt.Name = GetEventName(evnt.Event.GetType());
            evnt.Data = SerializeEvent(evnt.Event);
            OpenSession().Save(evnt);

            if (_logger.IsDebugEnabled)
            {
                string eventDetail = string.Format(
                    "AggregateRootName: {0}, aggregateRootId: {1}, Version: {2}, OccurredTime: {3}, EventName: {4}, EventData: {5}",
                    evnt.AggregateRootName,
                    evnt.AggregateRootId,
                    evnt.Version,
                    evnt.OccurredTime,
                    evnt.Name,
                    evnt.Data);
                _logger.DebugFormat("Inserted AggregateRootEvent. event detail:{0}", eventDetail);
            }
        }
        /// <summary>
        /// 锁定一个给定的聚合根，数据库级别的锁;
        /// 该方法内部不需要考虑事务，因为框架在调用该方法之前已经确保当前事务是开启的;
        /// 默认实现是通过ROWLOCK+UPDLOCK的方式实现行级锁;
        /// </summary>
        protected virtual long LockAggregateRootAfterTransactionStarted(string aggregateRootId)
        {
            var aggregateRootEventType = _aggregateRootEventTypeProvider.GetAggregateRootEventType(typeof(T));
            var aggregateRootName = GetAggregateRootName();

            DetachedCriteria detachedCriteria = DetachedCriteria.For(aggregateRootEventType)
                .Add(Restrictions.Eq("AggregateRootName", aggregateRootName))
                .Add(Restrictions.Eq("AggregateRootId", aggregateRootId))
                .Add(Subqueries.PropertyEq("Version",
                     DetachedCriteria.For(aggregateRootEventType)
                         .SetProjection(Projections.Max("Version"))
                         .Add(Restrictions.Eq("AggregateRootName", aggregateRootName))
                         .Add(Restrictions.Eq("AggregateRootId", aggregateRootId))));
            detachedCriteria.SetLockMode(LockMode.Upgrade);
            var criteria = detachedCriteria.GetExecutableCriteria(OpenSession());

            var eventList = typeof(EventQueryHelper).GetMethod("GetList").MakeGenericMethod(aggregateRootEventType).Invoke(new EventQueryHelper(), new object[] { criteria }) as IList;
            if (eventList != null && eventList.Count > 0)
            {
                return (long)((AggregateRootEvent)eventList[0]).Version;
            }
            return 0;
        }

        #endregion

        #region Private Methods

        private Type GetAggregateRootType(string aggregateRootName)
        {
            return _typeNameMapper.GetType(NameTypeMappingType.AggregateRootMapping, aggregateRootName);
        }
        private string GetAggregateRootName()
        {
            return _typeNameMapper.GetName(NameTypeMappingType.AggregateRootMapping, typeof(T));
        }
        private Type GetEventType(string eventName)
        {
            return _typeNameMapper.GetType(NameTypeMappingType.EventMapping, eventName);
        }
        private string GetEventName(Type eventType)
        {
            return _typeNameMapper.GetName(NameTypeMappingType.EventMapping, eventType);
        }
        private string SerializeEvent(object evnt)
        {
            return _eventSerializer.Serialize(evnt);
        }
        private object DeserializeEvent(Type eventType, string eventJson)
        {
            return _eventSerializer.Deserialize(eventJson, eventType);
        }

        #endregion

        private class EventQueryHelper
        {
            public IList<TAggregateRootEvent> GetList<TAggregateRootEvent>(ICriteria criteria) where TAggregateRootEvent : AggregateRootEvent
            {
                return criteria.List<TAggregateRootEvent>();
            }
        }
    }
}
