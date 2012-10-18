using System;
using System.Collections.Generic;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 工作单元接口定义，一个工作单元负责维护当前上下文中的一些新增或修改的聚合根
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 该属性用于当前的UnitOfWork被UnitOfWorkStore存储时，用于存储某个外部对象的引用。
        /// 此设计用于解决并发的情况下获取UnitOfWork可能会不正确的问题。
        /// </summary>
        object Cookie { get; set; }
        /// <summary>
        /// 跟踪某个给定的聚合根
        /// </summary>
        void TrackingAggregateRoot(AggregateRoot aggregateRoot);
        /// <summary>
        /// 返回当前所有被跟踪的聚合根
        /// </summary>
        IEnumerable<AggregateRoot> GetAllTrackingAggregateRoots();
        /// <summary>
        /// 提交当前所有被跟踪的聚合根上所发生的所有的事件；同时，通知基于同步模式的消息总线分发这些事件。
        /// <remarks>
        /// 在消息总线Publish事件的过程中，各种事件响应者比如Denormalizer会响应这些事件，做出相应的更新。
        /// </remarks>
        /// </summary>
        /// <returns>
        /// 返回所有已被处理的事件
        /// </returns>
        IEnumerable<object> SubmitChanges();
    }
}
