//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 消息总线接口定义
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
        /// <summary>
        /// 分发一个消息
        /// </summary>
        void Publish(object message);
        /// <summary>
        /// 分发多个消息
        /// </summary>
        void Publish(IEnumerable<object> messages);
        /// <summary>
        /// 注册消息订阅者
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RegisterSubscriber<T>();
        /// <summary>
        /// 注册消息订阅者
        /// </summary>
        /// <param name="subscriberType"></param>
        void RegisterSubscriber(Type subscriberType);
        /// <summary>
        /// 注册消息订阅者
        /// </summary>
        /// <param name="assemblies"></param>
        void RegisterAllSubscribersInAssemblies(params Assembly[] assemblies);
        /// <summary>
        /// 启动消息总线
        /// </summary>
        void Start();
    }
}
