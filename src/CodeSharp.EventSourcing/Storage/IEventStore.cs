//Copyright (c) CodeSharp.  All rights reserved.

using System.Collections.Generic;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// EventStore泛型接口定义，该接口能对某个特定类型的聚合根进行操作
    /// </summary>
    public interface IEventStore<T> where T : AggregateRoot
    {
        /// <summary>
        /// 根据给定聚合根ID以及事件版本号范围获取所有满足该条件的事件
        /// </summary>
        IEnumerable<AggregateRootEvent> GetEvents(string aggregateRootId, long minVersion, long maxVersion);
        /// <summary>
        /// 将给定的事件持久化
        /// </summary>
        void StoreEvents(IEnumerable<AggregateRootEvent> evnts);
        /// <summary>
        /// 锁住指定的聚合根，确保一个人在读当前聚合根的时候别人不允许读
        /// </summary>
        long LockAggregateRoot(string aggregateRootId);
    }
}
