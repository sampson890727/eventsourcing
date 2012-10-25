//Copyright (c) CodeSharp.  All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Castle.Services.Transaction;

namespace CodeSharp.EventSourcing.Castles
{
    /// <summary>
    /// 通过该类实现在Castle事务提交之前提交UnitOfWork的所有修改；事务提交之后，释放UnitOfWork。
    /// </summary>
    public class UnitOfWorkSynchronization : ISynchronization
    {
        private ITransaction _transaction;
        private UnitOfWorkDelegate _unitOfWorkDelegate;
        private IAsyncMessageBus _asyncMessageBus;
        private ILogger _logger;
        private IEnumerable<object> _events;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UnitOfWorkSynchronization(ITransaction transaction, UnitOfWorkDelegate unitOfWorkDelegate)
        {
            _transaction = transaction;
            _unitOfWorkDelegate = unitOfWorkDelegate;
            _asyncMessageBus = DependencyResolver.Resolve<IAsyncMessageBus>();
            _logger = DependencyResolver.Resolve<ILoggerFactory>().Create("EventSourcing.UnitOfWorkSynchronization");
        }

        public void BeforeCompletion()
        {
            _events = null;

            //ISynchronization在Castle事务的提交或回滚时都会被调用到，
            //而当在Castle事务回滚时，我们不需要执行UnitOfWork的SubmitChanges方法，
            //所以在这里需要加这个判断，IsRollbackOnlySet为true表示当前Castle的事务在回滚的过程中
            if (!_transaction.IsRollbackOnlySet)
            {
                var trackingAggregateRootCount = _unitOfWorkDelegate.GetAllTrackingAggregateRoots().Count();
                if (trackingAggregateRootCount > 0)
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("{0} submiting changes. Total tracked aggregateRoot count：{1}", _unitOfWorkDelegate.InnerUnitOfWork.GetType().Name, trackingAggregateRootCount);
                    }
                    _events = _unitOfWorkDelegate.SubmitChanges();
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("{0} submitted changes. Total tracked aggregateRoot count：{1}", _unitOfWorkDelegate.InnerUnitOfWork.GetType().Name, trackingAggregateRootCount);
                    }
                }
            }
        }
        public void AfterCompletion()
        {
            _unitOfWorkDelegate.InternalDispose();

            //只有当Castle事务提交成功时才需要异步分发事件
            if (!_transaction.IsRollbackOnlySet && _events != null && _events.Count() > 0)
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("{0} publishing events. Total events count：{1}", _asyncMessageBus.GetType().Name, _events.Count());
                }
                _asyncMessageBus.Publish(_events);
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("{0} published events. Total events count：{1}", _asyncMessageBus.GetType().Name, _events.Count());
                }
            }
        }
    }
}
