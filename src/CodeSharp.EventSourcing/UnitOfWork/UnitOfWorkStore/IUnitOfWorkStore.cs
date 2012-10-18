using System;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 表示一个存储UnitOfWork的Store，负责管理UnitOfWork的生命周期
    /// </summary>
    public interface IUnitOfWorkStore
    {
        /// <summary>
        /// 根据某个别名返回一个可用的UnitOfWork.
        /// </summary>
        IUnitOfWork FindCompatibleUnitOfWork(string alias);

        /// <summary>
        /// 存储某个UnitOfWork
        /// </summary>
        void Store(string alias, IUnitOfWork unitofWork);

        /// <summary>
        /// 移除指定的UnitOfWork
        /// </summary>
        void Remove(IUnitOfWork unitofWork);

        /// <summary>
        /// 判断某个别名对应的UnitOfWork是否存在
        /// </summary>
        bool IsCurrentActivityEmptyFor(string alias);
    }
}
