//Copyright (c) CodeSharp.  All rights reserved.

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 定义一个接口，允许用户获取一个当前上下文的UnitOfWork实例
    /// </summary>
    public interface IUnitOfWorkManager
    {
        /// <summary>
        /// 返回一个可用的UnitOfWork实例
        /// <remarks>
        /// 1. UnitOfWork有生命周期，每个IUnitOfWorkManager的实现类可以实现如何管理UnitOfWork的生命周期
        /// 2. UnitOfWork负责维护所有领域模型内聚合根发生的改变，只要有修改的包括新增的聚合根都会被UnitOfWork监控管理
        /// </remarks>
        /// </summary>
        IUnitOfWork GetUnitOfWork();
    }
}
