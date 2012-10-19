//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 用于提供聚合根中触发的事件的某个回调函数，该回调函数一定是聚合根内定义的某个函数
    /// </summary>
    public interface IAggregateRootEventCallbackMetaDataProvider
    {
        /// <summary>
        /// 根据聚合根的类型以及事件类型获取在聚合根上定义的响应该事件类型的响应函数委托实例
        /// </summary>
        Action<AggregateRoot, object> GetEventCallback(Type aggregateRootType, Type eventType);
        /// <summary>
        /// 注册指定程序集内的所有聚合根的内部事件响应函数
        /// </summary>
        /// <param name="assemblies"></param>
        void RegisterAllEventCallbackMetaDataInAssemblies(params Assembly[] assemblies);
    }
}
