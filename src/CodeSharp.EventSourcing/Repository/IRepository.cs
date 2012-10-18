using System;
using System.Collections.Generic;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 仓储接口定义
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// 往仓储中增加一个聚合根
        /// </summary>
        void Add(AggregateRoot aggregateRoot);
        /// <summary>
        /// 根据聚合根的唯一标识返回聚合根对象
        /// </summary>
        T GetById<T>(object aggregateRootId) where T : AggregateRoot;
        /// <summary>
        /// 根据聚合根的唯一标识返回聚合根对象，返回的聚合根会被Lock，
        /// Lock后在当前事务结束之前，被Lock的聚合根不允许被别人读取；
        /// 通过这个方法返回的聚合根能有效的防止并发带来的数据被覆盖的问题
        /// </summary>
        T GetByIdWithLock<T>(object aggregateRootId) where T : AggregateRoot;
        /// <summary>
        /// 根据给定的事件批量获取聚合根，通过重演事件方式重新得到聚合根，并将聚合根添加到UnitOfWork进行管理
        /// </summary>
        IList<T> GetFromEvents<T, TAggregateRootEvent>(IEnumerable<TAggregateRootEvent> evnts) where T : AggregateRoot where TAggregateRootEvent : AggregateRootEvent;
    }
}
