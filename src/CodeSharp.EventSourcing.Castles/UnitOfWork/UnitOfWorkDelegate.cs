using System;
using System.Collections.Generic;
using System.Threading;

namespace CodeSharp.EventSourcing.Castles
{
    /// <summary>
    /// 一个代理类，实现了IUnitOfWork接口，内部封装了一个IUnitOfWork实例，
    /// 所有IUnitOfWork接口定义的属性或方法都转交给内部维护的IUnitOfWork实例实现。
    /// 定义该代理的目的是为了当UnitOfWork外层存在Castle事务的情况下，
    /// 当前UnitOfWorkDelegate对象不会自动被从UnitOfWorkStore中移除，
    /// 而是由外层的Castle事务来决定何时该释放当前的UnitOfWorkDelegate。
    /// </summary>
    public class UnitOfWorkDelegate : MarshalByRefObject, IUnitOfWork
    {
        #region Private Variables

        private IUnitOfWork _unitOfWork;
        private IUnitOfWorkStore _unitofWorkStore;
        private ILogger _logger;
        private bool _canAutoDispose;
        private bool _isDisposed;
        private readonly ReaderWriterLockSlim _readerWriterLockSlim;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitOfWork">内部封装的UnitOfWork</param>
        /// <param name="unitofWorkStore">存储UnitOfWork的Store</param>
        /// <param name="loggerFactory">Log Factory</param>
        /// <param name="canAutoDispose">表示当前的UnitOfWorkDelegate对象是否允许在Dispose方法被调用时自动从UnitOfWorkStore中移除</param>
        public UnitOfWorkDelegate(IUnitOfWork unitOfWork, IUnitOfWorkStore unitofWorkStore, ILoggerFactory loggerFactory, bool canAutoDispose)
        {
            _unitOfWork = unitOfWork;
            _unitofWorkStore = unitofWorkStore;
            _logger = loggerFactory.Create("EventSourcing.UnitOfWorkDelegate");
            _canAutoDispose = canAutoDispose;
            _isDisposed = false;
            _readerWriterLockSlim = new ReaderWriterLockSlim();
        }

        #endregion

        #region Public & Internal Properties

        /// <summary>
        /// 返回内部封装的UnitOfWork对象
        /// </summary>
        public IUnitOfWork InnerUnitOfWork
        {
            get { return _unitOfWork; }
        }
        /// <summary>
        /// 一个对象，目前为一个Stack实例。在将当前UnitOfWorkDelegate对象从UnitOfWorkStore中移除时将会用到这个属性，
        /// 详情见AbstractUnitOfWorkStore的Remove方法。
        /// </summary>
        public object Cookie
        {
            get { return _unitOfWork.Cookie; }
            set { _unitOfWork.Cookie = value; }
        }

        #endregion

        #region IUnitOfWork 接口实现

        /// <summary>
        /// 跟踪某个聚合根
        /// </summary>
        public void TrackingAggregateRoot(AggregateRoot aggregateRoot)
        {
            _unitOfWork.TrackingAggregateRoot(aggregateRoot);
        }

        /// <summary>
        /// 返回所有当前跟踪的聚合根
        /// </summary>
        public IEnumerable<AggregateRoot> GetAllTrackingAggregateRoots()
        {
            return _unitOfWork.GetAllTrackingAggregateRoots();
        }

        /// <summary>
        /// 提交当前跟踪的所有聚合根上所发生的所有事件
        /// </summary>
        /// <returns>
        /// 返回所有已被处理的事件
        /// </returns>
        public IEnumerable<object> SubmitChanges()
        {
            return _unitOfWork.SubmitChanges();
        }

        /// <summary>
        /// 如果当前UnitOfWork外面没有被事务所封装，则释放自己；否则，不做任何事情。
        /// </summary>
        public void Dispose()
        {
            if (_canAutoDispose)
            {
                InternalDispose();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// 更新是否可以被自动Dispose的标记位
        /// </summary>
        internal void UpdateCanAutoDispose(bool canAutoDispose)
        {
            _readerWriterLockSlim.AtomWrite(
                () =>
                {
                    _canAutoDispose = canAutoDispose;
                }
            );
        }
        /// <summary>
        /// 程序集内部方法，只允许框架内部调用；
        /// 当Castle的事务在提交完成后会调用该方法将当前的UnitOfWork从UnitOfWorkStore中移除
        /// </summary>
        internal void InternalDispose()
        {
            if (!_isDisposed)
            {
                 _unitofWorkStore.Remove(this);
                _unitOfWork.Dispose();
                _isDisposed = true;
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Disposed {0}.", _unitOfWork.GetType().Name);
                }
            }
        }

        #endregion
    }
}