//Copyright (c) CodeSharp.  All rights reserved.

using System;
using Castle.Services.Transaction;

namespace CodeSharp.EventSourcing.Castles
{
    /// <summary>
    /// 基于Castle的事务框架实现的UnitOfWorkManager
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
        /// <remarks>
        /// 1. UnitOfWork负责维护所有领域模型内聚合根发生的改变，只要有修改的包括新增的聚合根都会被UnitOfWork监控管理
        /// 2. UnitOfWork的生命周期与Castle的事务的生命周期一致，即在Castle事务提交之前UnitOfWork会将所有聚合根的修改提交到数据库;
        /// </remarks>
        /// </summary>
        public IUnitOfWork GetUnitOfWork()
        {
            var unitOfWorkDelegate = _unitofWorkStore.FindCompatibleUnitOfWork(DefaultAlias) as UnitOfWorkDelegate;
            var currentTransaction = GetCurrentTransaction();
            bool hasTransaction = currentTransaction != null;

            if (unitOfWorkDelegate == null)
            {
                unitOfWorkDelegate = CreateUnitOfWorkDelegate(CreateUnitOfWork(), hasTransaction);
                RegisterUnitOfWorkSynchronization(currentTransaction, unitOfWorkDelegate);
                _unitofWorkStore.Store(DefaultAlias, unitOfWorkDelegate);
            }
            else
            {
                unitOfWorkDelegate.UpdateCanAutoDispose(!hasTransaction);
            }

            return unitOfWorkDelegate;
        }

        /// <summary>
        /// 从ITransactionManager获取当前Castle事务
        /// </summary>
        private ITransaction GetCurrentTransaction()
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
        private void RegisterUnitOfWorkSynchronization(ITransaction transaction, UnitOfWorkDelegate unitofWorkDelegate)
        {
            if (transaction != null && !transaction.IsChildTransaction)
            {
                transaction.RegisterSynchronization(new UnitOfWorkSynchronization(transaction, unitofWorkDelegate));
            }
        }
    }
}