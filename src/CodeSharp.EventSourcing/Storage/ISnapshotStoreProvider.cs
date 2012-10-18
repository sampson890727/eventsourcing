using System.Collections.Generic;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 一个接口用于返回一个SnapshotStore实例
    /// </summary>
    public interface ISnapshotStoreProvider
    {
        /// <summary>
        /// 返回一个SnapshotStore实例
        /// </summary>
        ISnapshotStore<TAggregateRoot> GetSnapshotStore<TAggregateRoot>() where TAggregateRoot : AggregateRoot;
    }
}
