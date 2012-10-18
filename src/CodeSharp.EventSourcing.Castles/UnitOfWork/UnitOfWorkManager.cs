using System;
using Castle.Services.Transaction;
using ICastleTransaction = Castle.Services.Transaction.ITransaction;

namespace CodeSharp.EventSourcing.Castles
{
    /// <summary>
    /// 基于Castle实现的UnitOfWorkManager
    /// </summary>
    public class UnitOfWorkManager : MarshalByRefObject, IUnitOfWorkManager
    {
        private readonly string DefaultAlias = "codesharp.eventsourcing.unitofworkmanager.castle";
        private readonly IUnitOfWorkStore _unitofWorkStore;
        private ILoggerFactory _loggerFactory;
        private ILogger _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UnitOfWorkManager(IUnitOfWorkStore unitofWorkStore, ILoggerFactory loggerFactory)
        {
            _unitofWorkStore = unitofWorkStore;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.Create("EventSourcing.UnitOfWorkManager");
        }

        /// <summary>
        /// 返回一个当前可用的UnitOfWork实例
        /// </summary>
        /// <remarks>
        /// UnitOfWork的生命周期和NHibernate Session的生命周期一致；
        /// 生命周期描述如下：
        /// 1）如果是用户采用了Castle的Transaction特性的情况，则UnitOfWork的生命周期与Castle的事务的生命周期一致；
        /// 2）如果是用户不采用Castle的Transaction特性的情况，则UnitOfWork的生命周期就完全和UnitOfWorkStore定义的生命周期一致；
        /// 需要说明的是：虽然NHibernate Session或者UnitOfWork都是通过ISessionStore或IUnitOfWorkStore来存储的，所以从存储时间的角度来说
        /// 如果没有主动把Session或UnitOfWork从Store中移除，那么这两个对象的生命周期就是CallContext或者整个HttpContext的生命周期；
        /// 但是因为我们这里用到了Castle的事务，所以这两个对象的生命周期就不再是整个CallContext或者HttpContext了，而是与Castle的顶层事务TalkativeTransaction
        /// 的生命周期一致，当TalkativeTransaction Commit完成后，会调用其上面所注册的所有的ISynchronization对象的AfterCompletion方法；
        /// 目前NHibernate的SessionDisposeSynchronization类会注册到Castle的TalkativeTransaction，所以会被调用，这个类的AfterCompletion方法会
        /// 将Session从其所属的ISessionStore中移除；
        /// 同样，Event Sourcing框架也设计了一个UnitOfWorkSynchronization类，这个类也会在那时将UnitOfWork从其所属的UnitOfWorkStore中移除；
        /// </remarks>
        public IUnitOfWork GetUnitOfWork()
        {
            UnitOfWorkDelegate unitOfWorkDelegate = _unitofWorkStore.FindCompatibleUnitOfWork(DefaultAlias) as UnitOfWorkDelegate;
            ICastleTransaction currentTransaction = GetCurrentCastleTransaction();
            bool hasCastleTransaction = currentTransaction != null;

            if (unitOfWorkDelegate == null)
            {
                unitOfWorkDelegate = CreateUnitOfWorkDelegate(CreateUnitOfWork(), hasCastleTransaction);
                RegisterUnitOfWorkSynchronization(currentTransaction, unitOfWorkDelegate);
                _unitofWorkStore.Store(DefaultAlias, unitOfWorkDelegate);
            }
            else
            {
                unitOfWorkDelegate.UpdateCanAutoDispose(!hasCastleTransaction);
            }

            return unitOfWorkDelegate;
        }

        /// <summary>
        /// 从ITransactionManager获取当前Castle事务
        /// </summary>
        private ICastleTransaction GetCurrentCastleTransaction()
        {
            return DependencyResolver.Resolve<ITransactionManager>().CurrentTransaction;
        }
        /// <summary>
        /// 返回一个UnitOfWork实例
        /// </summary>
        private IUnitOfWork CreateUnitOfWork()
        {
            return DependencyResolver.Resolve<IUnitOfWork>();
        }
        /// <summary>
        /// 封装UnitOfWork
        /// </summary>
        private UnitOfWorkDelegate CreateUnitOfWorkDelegate(IUnitOfWork unitofWork, bool hasCastleTransaction)
        {
            return new UnitOfWorkDelegate(unitofWork, _unitofWorkStore, _loggerFactory, !hasCastleTransaction);
        }
        /// <summary>
        /// 注册一个UnitOfWorkSynchronization事务同步对象到当前Castle的顶层事务中
        /// </summary>
        private void RegisterUnitOfWorkSynchronization(ICastleTransaction transaction, UnitOfWorkDelegate unitofWorkDelegate)
        {
            if (transaction != null && !transaction.IsChildTransaction)
            {
                transaction.RegisterSynchronization(new UnitOfWorkSynchronization(transaction, unitofWorkDelegate));
            }
        }
    }
}