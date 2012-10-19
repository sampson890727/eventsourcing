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
        /// </summary>
        IUnitOfWork GetUnitOfWork();
    }
}
