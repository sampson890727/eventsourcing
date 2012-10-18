using System;
using System.Linq;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 默认快照实现类
    /// </summary>
    public class DefaultSnapshotter : ISnapshotter
    {
        #region Private Variables

        private IAggregateRootFactory _aggregateRootFactory;

        #endregion

        #region Constructors

        public DefaultSnapshotter(IAggregateRootFactory aggregateRootFactory)
        {
            _aggregateRootFactory = aggregateRootFactory;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 为给定的聚合根创建快照
        /// </summary>
        public virtual Snapshot CreateSnapshot(AggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }

            if (!IsSnapshotable(aggregateRoot))
            {
                throw new InvalidOperationException(string.Format("聚合根({0})没有实现ISnapshotable接口或者实现了多余1个的ISnapshotable接口，不能对其创建快照。", aggregateRoot.GetType().FullName));
            }

            var snapshotDataType = GetSnapshotType(aggregateRoot);
            var internalSnapshotterType = typeof(InternalSnapshotter<>).MakeGenericType(snapshotDataType);
            var snapshotData = internalSnapshotterType.GetMethod("CreateSnapshot").Invoke(Activator.CreateInstance(internalSnapshotterType), new object[] { aggregateRoot });

            return new Snapshot(
                aggregateRoot.GetType(),
                aggregateRoot.UniqueId,
                aggregateRoot.GetOriginalVersion(),
                snapshotData,
                DateTime.Now);
        }
        /// <summary>
        /// 从给定的快照还原聚合根
        /// </summary>
        public virtual AggregateRoot RestoreFromSnapshot(Snapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid())
            {
                return null;
            }

            AggregateRoot aggregateRoot = _aggregateRootFactory.CreateAggregateRoot(snapshot.AggregateRootType);
            if (!IsSnapshotable(aggregateRoot))
            {
                throw new InvalidOperationException(string.Format("聚合根({0})没有实现ISnapshotable接口或者实现了多余1个的ISnapshotable接口，不能将其从某个快照还原。", aggregateRoot.GetType().FullName));
            }

            if (GetSnapshotType(aggregateRoot) != snapshot.Data.GetType())
            {
                throw new InvalidOperationException(string.Format("当前聚合根的快照类型({0})与要还原的快照类型({1})不符", GetSnapshotType(aggregateRoot), snapshot.Data.GetType()));
            }

            aggregateRoot.InitializeFromSnapshot(snapshot);

            var internalSnapshotterType = typeof(InternalSnapshotter<>).MakeGenericType(snapshot.Data.GetType());
            internalSnapshotterType.GetMethod("RestoreFromSnapshot").Invoke(Activator.CreateInstance(internalSnapshotterType), new object[] { aggregateRoot, snapshot.Data });

            return aggregateRoot;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 判断某个聚合根是否支持快照
        /// </summary>
        private bool IsSnapshotable(AggregateRoot aggregateRoot)
        {
            return aggregateRoot.GetType().GetInterfaces().Count(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISnapshotable<>)) == 1;
        }
        /// <summary>
        /// 返回聚合根的快照类型
        /// </summary>
        private Type GetSnapshotType(AggregateRoot aggregateRoot)
        {
            return aggregateRoot.GetType().GetInterfaces().Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISnapshotable<>)).GetGenericArguments()[0];
        }
        /// <summary>
        /// 内部辅助类，用于创建和还原快照
        /// </summary>
        private class InternalSnapshotter<TSnapshot>
        {
            public TSnapshot CreateSnapshot(AggregateRoot aggregateRoot)
            {
                return ((ISnapshotable<TSnapshot>)aggregateRoot).CreateSnapshot();
            }
            public void RestoreFromSnapshot(AggregateRoot aggregateRoot, TSnapshot snapshot)
            {
                ((ISnapshotable<TSnapshot>)aggregateRoot).RestoreFromSnapshot(snapshot);
            }
        }

        #endregion
    }
}
