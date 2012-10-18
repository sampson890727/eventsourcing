using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 仓储接口默认实现
    /// </summary>
    [Transactional]
    public class Repository : IRepository
    {
        /// <summary>
        /// 添加一个聚合根实例到仓储中
        /// </summary>
        [Transaction]
        public virtual void Add(AggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }
            InvokeRepositoryMethod(CreateGenericRepository(aggregateRoot.GetType()), "Add", aggregateRoot);
        }
        /// <summary>
        /// 根据给定的聚合根ID返回一个唯一的聚合根实例
        /// </summary>
        [Transaction]
        public virtual T GetById<T>(object aggregateRootId) where T : AggregateRoot
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            return InvokeRepositoryMethod(CreateGenericRepository(typeof(T)), "GetById", aggregateRootId.ToString()) as T;
        }
        /// <summary>
        /// 根据给定的聚合根ID返回一个唯一的聚合根实例
        /// 返回之前会先在数据库级别锁住该聚合根，锁的级别是行级排它锁(ROWLOCK,XLOCK)，
        /// 确保你读取之后，事务执行完成之前，别人都不允许读取该聚合根，也就是不会出现脏读
        /// </summary>
        [Transaction]
        public virtual T GetByIdWithLock<T>(object aggregateRootId) where T : AggregateRoot
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            return InvokeRepositoryMethod(CreateGenericRepository(typeof(T)), "GetByIdWithLock", aggregateRootId.ToString()) as T;
        }
        /// <summary>
        /// 根据给定的事件批量获取聚合根，通过重演事件方式重新得到聚合根，并将聚合根添加到UnitOfWork进行管理
        /// </summary>
        [Transaction]
        public virtual IList<T> GetFromEvents<T, TAggregateRootEvent>(IEnumerable<TAggregateRootEvent> evnts) where T : AggregateRoot where TAggregateRootEvent : AggregateRootEvent
        {
            if (evnts == null)
            {
                throw new ArgumentNullException("evnts");
            }
            return InvokeRepositoryMethod(CreateGenericRepository(typeof(T)), "GetFromEvents", evnts) as IList<T>;
        }

        /// <summary>
        /// 的Repository对象实例
        /// </summary>
        private object CreateGenericRepository(Type aggregateRootType)
        {
            return Activator.CreateInstance(typeof(GenericRepository<>).MakeGenericType(aggregateRootType));
        }
        /// <summary>
        /// 调用IRepository对象上指定名称的方法
        /// </summary>
        private object InvokeRepositoryMethod(object repository, string methodName, params object[] parameters)
        {
            return repository.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public).Single(x => x.Name == methodName).Invoke(repository, parameters);
        }
    }
}
