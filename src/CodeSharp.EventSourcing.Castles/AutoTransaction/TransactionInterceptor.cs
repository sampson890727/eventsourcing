//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Reflection;
using Castle.Core;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.Services.Transaction;

namespace CodeSharp.EventSourcing.Castles
{
    [Transient]
    public class TransactionInterceptor : IInterceptor, IOnBehalfAware
    {
        private readonly IKernel _kernel;
        private readonly TransactionMetaInfoStore _infoStore;
        private TransactionMetaInfo _metaInfo;
        private ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionInterceptor"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        /// <param name="infoStore">The info store.</param>
        public TransactionInterceptor(IKernel kernel, TransactionMetaInfoStore infoStore, ILoggerFactory loggerFactory)
        {
            this._kernel = kernel;
            this._infoStore = infoStore;
            this._logger = loggerFactory.Create("EventSourcing.TransactionInterceptor");
        }

        /// <summary>
        /// Sets the intercepted component's ComponentModel.
        /// </summary>
        /// <param name="target">The target's ComponentModel</param>
        public void SetInterceptedComponentModel(ComponentModel target)
        {
            _metaInfo = _infoStore.GetMetaFor(target.Implementation);
        }
        /// <summary>
        /// Intercepts the specified invocation and creates a transaction if necessary.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        /// <returns></returns>
        public void Intercept(IInvocation invocation)
        {
            MethodInfo methodInfo;
            if (invocation.Method.DeclaringType.IsInterface)
            {
                methodInfo = invocation.MethodInvocationTarget;
            }
            else
            {
                methodInfo = invocation.Method;
            }

            if (_metaInfo == null || !_metaInfo.Contains(methodInfo))
            {
                invocation.Proceed();
                return;
            }

            ITransactionManager transactionManager = _kernel.Resolve<ITransactionManager>();
            ITransaction transaction = transactionManager.CreateTransaction(TransactionMode.Requires, IsolationMode.ReadCommitted, false);

            if (transaction == null)
            {
                invocation.Proceed();
                return;
            }

            transaction.Begin();

            bool rolledback = false;

            try
            {
                invocation.Proceed();

                if (transaction.IsRollbackOnlySet)
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Rolling back castle transaction {0}", transaction.GetHashCode());
                    }
                    rolledback = true;
                    transaction.Rollback();
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Rolled back castle transaction {0}", transaction.GetHashCode());
                    }
                }
                else
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Committing castle transaction {0}", transaction.GetHashCode());
                    }
                    transaction.Commit();
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Committed castle transaction {0}", transaction.GetHashCode());
                    }
                }
            }
            catch (TransactionException ex)
            {
                _logger.Fatal("Fatal error during castle transaction processing", ex);
                throw;
            }
            catch (Exception)
            {
                if (!rolledback)
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Rolling back castle transaction {0} due to exception on method {2}.{1}", transaction.GetHashCode(), methodInfo.Name, methodInfo.DeclaringType.Name);
                    }
                    rolledback = true;
                    transaction.Rollback();
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.DebugFormat("Rolled back castle transaction {0} due to exception on method {2}.{1}", transaction.GetHashCode(), methodInfo.Name, methodInfo.DeclaringType.Name);
                    }
                }
                throw;
            }
            finally
            {
                transactionManager.Dispose(transaction);
            }
        }
    }
}
