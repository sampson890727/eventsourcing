using System;

namespace CodeSharp.EventSourcing
{
    public interface ISnapshotTypeProvider
    {
        /// <summary>
        /// 返回聚合根对应的Snapshot的类型
        /// </summary>
        Type GetSnapshotType(Type aggregateRootType);
    }
}
