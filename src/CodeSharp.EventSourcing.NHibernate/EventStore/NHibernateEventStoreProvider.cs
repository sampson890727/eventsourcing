//Copyright (c) CodeSharp.  All rights reserved.

namespace CodeSharp.EventSourcing.NHibernate
{
    public class NHibernateEventStoreProvider : IEventStoreProvider
    {
        /// <summary>
        /// 先判断容器中是否注册了用户自定义的EventStore, 如果没有，则返回一个NHibernateEventStoreBase实例
        /// </summary>
        public IEventStore<TAggregateRoot> GetEventStore<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            if (DependencyResolver.IsTypeRegistered(typeof(IEventStore<TAggregateRoot>)))
            {
                return DependencyResolver.Resolve<IEventStore<TAggregateRoot>>();
            }
            else
            {
                return new NHibernateEventStoreBase<TAggregateRoot>();
            }
        }
    }
}
