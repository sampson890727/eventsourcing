//Copyright (c) CodeSharp.  All rights reserved.

using Castle.Services.Transaction;

namespace CodeSharp.EventSourcing.Castles
{
    /// <summary>
    /// EventSourcing框架中用到的TransactionManager，自动会监听Castle.Services.Transaction.DefaultTransactionManager
    /// 的TransactionCreated事件，如果当前创建的事务是顶层父事务，则自动激活UnitOfWork
    /// </summary>
    public class TransactionManager : DefaultTransactionManager, ITransactionManager
    {
        private ILogger _logger = DependencyResolver.Resolve<ILoggerFactory>().Create("EventSourcing.TransactionManager");

        public TransactionManager() : base()
        {
            Initialize();
        }
        public TransactionManager(IActivityManager activityManager) : base(activityManager)
        {
            Initialize();
        }

        private void Initialize()
        {
            // 在Castle的顶层事务创建时激活UnitOfWork，确保UnitOfWork附加在Castle的顶层事务上；
            // UnitOfWork必须附加在顶层事务上，因为只有顶层事务才会被真正Commit；
            // 如果UnitOfWork附加在Castle的子事务上，那么UnitOfWork的修改将无法自动提交。
            TransactionCreated += (sender, e) =>
            {
                DependencyResolver.Resolve<IUnitOfWorkManager>().GetUnitOfWork();
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("{0} Created, Transaction Name:{1}.", e.Transaction.GetType().Name, e.Transaction.GetHashCode());
                }
            };

            //Castle子事务创建时记录日志
            ChildTransactionCreated += (sender, e) =>
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("{0} Created, Transaction Name:{1}.", e.Transaction.GetType().Name, e.Transaction.GetHashCode());
                }
            };
        }
    }
}