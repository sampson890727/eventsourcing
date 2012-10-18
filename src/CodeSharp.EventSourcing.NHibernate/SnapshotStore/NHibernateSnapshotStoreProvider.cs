namespace CodeSharp.EventSourcing.NHibernate
{
    public class NHibernateSnapshotStoreProvider : ISnapshotStoreProvider
    {
        /// <summary>
        /// 先判断容器中是否注册了用户自定义的SnapshotStore, 如果没有，则返回一个NHibernateSnapshotStoreBase实例
        /// </summary>
        public ISnapshotStore<TAggregateRoot> GetSnapshotStore<TAggregateRoot>() where TAggregateRoot : AggregateRoot
        {
            if (DependencyResolver.IsTypeRegistered(typeof(ISnapshotStore<TAggregateRoot>)))
            {
                return DependencyResolver.Resolve<ISnapshotStore<TAggregateRoot>>();
            }
            else
            {
                return new NHibernateSnapshotStoreBase<TAggregateRoot>();
            }
        }
    }
}
