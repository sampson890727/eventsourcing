using System.Collections.Generic;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 一个接口用于返回一个EventStore实例
    /// </summary>
    public interface IEventStoreProvider
    {
        /// <summary>
        /// 返回一个EventStore实例
        /// </summary>
        IEventStore<TAggregateRoot> GetEventStore<TAggregateRoot>() where TAggregateRoot : AggregateRoot;
    }
}
