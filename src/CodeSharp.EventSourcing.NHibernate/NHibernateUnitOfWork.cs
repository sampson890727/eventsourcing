using NHibernate;

namespace CodeSharp.EventSourcing.NHibernate
{
    /// <summary>
    /// 基于NHibernate实现的IUnitOfWork
    /// </summary>
    public class NHibernateUnitOfWork : DefaultUnitOfWork
    {
        public NHibernateUnitOfWork(INHibernateSessionManager sessionManager, ISyncMessageBus eventBus, ILoggerFactory loggerFactory, IDomainHandlerMetaDataProvider metaDataProvider)
            : base(eventBus, loggerFactory, metaDataProvider)
        {
            //这里在构造函数中激活NHibernate的Session，
            //这样做的目的是确保在Castle提交事务时，不会有新的Session注册到Castle的事务上；
            //Castle不允许在提交事务的过程中再有创建Session的情况出现，否则会抛出资源竞争并发冲突异常。
            sessionManager.OpenSession();
        }
    }
}
