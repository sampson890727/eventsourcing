using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 用于提供聚合根作为订阅者的聚合根内的事件响应函数的元数据信息
    /// </summary>
    public interface IDomainHandlerMetaDataProvider
    {
        /// <summary>
        /// 返回给定事件对应的所有聚合根事件响应函数的元数据信息
        /// </summary>
        IEnumerable<DomainHandlerMetaData> GetMetaDatas(object evnt);
        /// <summary>
        /// 注册领域模型内的一个事件订阅者，订阅者必须是聚合根
        /// </summary>
        /// <param name="subscriberType"></param>
        void RegisterSubscriber(Type subscriberType);
        /// <summary>
        /// 注册指定程序集内的领域模型内的所有聚合根类型的事件订阅者
        /// </summary>
        /// <param name="assemblies"></param>
        void RegisterAllEventSubscribersInAssemblies(params Assembly[] assemblies);
    }
}
