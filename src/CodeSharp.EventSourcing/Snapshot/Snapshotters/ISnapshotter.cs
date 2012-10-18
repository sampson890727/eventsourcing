using System;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 定义一个快照解析器接口，规范聚合根的快照的创建和还原的标准行为：
    /// 1）根据给定的聚合根创建快照
    /// 2）根据给定的快照还原聚合根
    /// </summary>
    public interface ISnapshotter
    {
        /// <summary>
        /// 为给定的聚合根创建快照
        /// </summary>
        Snapshot CreateSnapshot(AggregateRoot aggregateRoot);
        /// <summary>
        /// 从给定的快照还原聚合根
        /// </summary>
        AggregateRoot RestoreFromSnapshot(Snapshot snapshot);
    }
}
