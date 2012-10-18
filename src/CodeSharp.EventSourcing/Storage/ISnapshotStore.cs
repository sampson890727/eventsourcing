namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 表示事件快照的持久化存储设备
    /// </summary>
    public interface ISnapshotStore<T> where T : AggregateRoot
    {
        /// <summary>
        /// 将给定的快照持久化
        /// </summary>
        void StoreShapshot(Snapshot snapshot);
        /// <summary>
        /// 获取指定聚合根的相关快照中版本号小于给定版本号的最后一个快照
        /// </summary>
        Snapshot GetLastSnapshot(string aggregateRootId, long maxVersion);
        /// <summary>
        /// 获取指定聚合根的相关快照中版本号等于给定版本号的唯一一个快照
        /// </summary>
        Snapshot GetSingleSnapshot(string aggregateRootId, long requiredVersion);
    }
}
