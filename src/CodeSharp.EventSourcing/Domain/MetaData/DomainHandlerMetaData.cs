//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 封装一个聚合根的消息响应方法的元数据信息
    /// </summary>
    public class DomainHandlerMetaData : HandlerMetaData
    {
        /// <summary>
        /// 能够获取聚合根实例的一个路径集合
        /// </summary>
        public IEnumerable<Path> Paths { get; set; }
        /// <summary>
        /// 要响应消息的聚合根是否通过Lock的方式取出来
        /// </summary>
        public bool GetWithLock { get; set; }
    }
}