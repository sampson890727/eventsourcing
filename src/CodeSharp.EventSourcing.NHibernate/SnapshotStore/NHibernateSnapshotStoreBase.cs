using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;

namespace CodeSharp.EventSourcing.NHibernate
{
    /// <summary>
    /// ISnapshotStore接口的基于NHiberate实现的基类
    /// </summary>
    [Transactional]
    public class NHibernateSnapshotStoreBase<T> : ISnapshotStore<T> where T : AggregateRoot
    {
        #region Private Variables

        private INHibernateSessionManager _sessionManager;
        private IJsonSerializer _snapshotSerializer;
        private ITypeNameMapper _typeNameMapper;
        private ISnapshotTypeProvider _snapshotTypeProvider;
        private ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public NHibernateSnapshotStoreBase()
            : this(
            DependencyResolver.Resolve<INHibernateSessionManager>(),
            DependencyResolver.Resolve<IJsonSerializer>(),
            DependencyResolver.Resolve<ITypeNameMapper>(),
            DependencyResolver.Resolve<ISnapshotTypeProvider>(),
            DependencyResolver.Resolve<ILoggerFactory>())
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public NHibernateSnapshotStoreBase(
            INHibernateSessionManager sessionManager,
            IJsonSerializer snapshotSerializer,
            ITypeNameMapper typeNameMapper,
            ISnapshotTypeProvider snapshotTypeProvider,
            ILoggerFactory loggerFactory)
        {
            _sessionManager = sessionManager;
            _snapshotSerializer = snapshotSerializer;
            _typeNameMapper = typeNameMapper;
            _snapshotTypeProvider = snapshotTypeProvider;
            _logger = loggerFactory.Create("EventSourcing.NHibernateSnapshotStoreBase");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 持久化一个快照
        /// </summary>
        [Transaction]
        public virtual void StoreShapshot(Snapshot snapshot)
        {
            snapshot.AggregateRootName = GetAggregateRootName();
            snapshot.Name = GetSnapshotDataName(snapshot.Data.GetType());
            snapshot.SerializedData = SerializeSnapshotData(snapshot.Data);
            OpenSession().SaveOrUpdate(snapshot);
        }
        /// <summary>
        /// 获取指定聚合根的相关快照中版本号小于给定版本号的最后一个快照
        /// </summary>
        [Transaction]
        public virtual Snapshot GetLastSnapshot(string aggregateRootId, long maxVersion)
        {
            //组装查询Criteria
            Type snapshotType = _snapshotTypeProvider.GetSnapshotType(typeof(T));
            ICriteria criteria = OpenSession().CreateCriteria(snapshotType);
            criteria.Add(Restrictions.Eq("aggregateRootId", aggregateRootId));
            criteria.Add(Restrictions.Eq("AggregateRootName", GetAggregateRootName()));
            criteria.Add(Restrictions.Le("Version", maxVersion));
            criteria.AddOrder(new Order("Version", false));

            //取TOP 1
            criteria = criteria.SetFirstResult(0).SetMaxResults(1);

            //执行查询操作
            var snapshotList = typeof(SnapshotQueryHelper).GetMethod("GetList").MakeGenericMethod(snapshotType).Invoke(new SnapshotQueryHelper(), new object[] { criteria }) as IList;

            //转换查询结果
            if (snapshotList.Count > 0)
            {
                var snapshot = snapshotList[0] as Snapshot;
                snapshot.AggregateRootType = GetaggregateRootType(snapshot.AggregateRootName);
                snapshot.Data = DeserializeSnapshotData(GetSnapshotDataType(snapshot.Name), snapshot.SerializedData);
                return snapshot;
            }

            return null;
        }
        /// <summary>
        /// 获取指定聚合根的相关快照中版本号等于给定版本号的唯一一个快照
        /// </summary>
        [Transaction]
        public virtual Snapshot GetSingleSnapshot(string aggregateRootId, long requiredVersion)
        {
            //组装查询Criteria
            Type snapshotType = _snapshotTypeProvider.GetSnapshotType(typeof(T));
            ICriteria criteria = OpenSession().CreateCriteria(snapshotType);
            criteria.Add(Restrictions.Eq("aggregateRootId", aggregateRootId));
            criteria.Add(Restrictions.Eq("AggregateRootName", GetAggregateRootName()));
            criteria.Add(Restrictions.Eq("Version", requiredVersion));

            //执行查询操作
            var snapshotList = typeof(SnapshotQueryHelper).GetMethod("GetList").MakeGenericMethod(snapshotType).Invoke(new SnapshotQueryHelper(), new object[] { criteria }) as IList;

            //转换查询结果
            if (snapshotList.Count > 0)
            {
                var snapshot = snapshotList[0] as Snapshot;
                snapshot.AggregateRootType = GetaggregateRootType(snapshot.AggregateRootName);
                snapshot.Data = DeserializeSnapshotData(GetSnapshotDataType(snapshot.Name), snapshot.SerializedData);
                return snapshot;
            }

            return null;
        }

        #endregion

        #region Protected Methods

        protected ISession OpenSession()
        {
            return _sessionManager.OpenSession();
        }

        #endregion

        #region Private Methods

        private Type GetaggregateRootType(string aggregateRootName)
        {
            return _typeNameMapper.GetType(NameTypeMappingType.AggregateRootMapping, aggregateRootName);
        }
        private string GetAggregateRootName()
        {
            return _typeNameMapper.GetName(NameTypeMappingType.AggregateRootMapping, typeof(T));
        }
        private Type GetSnapshotDataType(string snapshotDataName)
        {
            return _typeNameMapper.GetType(NameTypeMappingType.SnapshotMapping, snapshotDataName);
        }
        private string GetSnapshotDataName(Type snapshotDataType)
        {
            return _typeNameMapper.GetName(NameTypeMappingType.SnapshotMapping, snapshotDataType);
        }
        private string SerializeSnapshotData(object snapshotData)
        {
            return _snapshotSerializer.Serialize(snapshotData);
        }
        private object DeserializeSnapshotData(Type snapshotDataType, string snapshotSerializedData)
        {
            return _snapshotSerializer.Deserialize(snapshotSerializedData, snapshotDataType);
        }

        #endregion

        private class SnapshotQueryHelper
        {
            public IList<TSnapshot> GetList<TSnapshot>(ICriteria criteria) where TSnapshot : Snapshot
            {
                return criteria.List<TSnapshot>();
            }
        }
    }
}
