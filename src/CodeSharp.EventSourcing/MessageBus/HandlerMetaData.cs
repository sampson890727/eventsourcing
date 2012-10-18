using System;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 封装一个消息响应方法的元数据信息
    /// </summary>
    public class HandlerMetaData
    {
        /// <summary>
        /// 响应方法
        /// </summary>
        public MethodInfo Handler { get; set; }
        /// <summary>
        /// 订阅者的类型
        /// </summary>
        public Type SubscriberType { get; set; }
    }
}