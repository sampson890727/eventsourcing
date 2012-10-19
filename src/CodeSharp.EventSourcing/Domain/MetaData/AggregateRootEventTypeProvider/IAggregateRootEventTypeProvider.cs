//Copyright (c) CodeSharp.  All rights reserved.

using System;

namespace CodeSharp.EventSourcing
{
    public interface IAggregateRootEventTypeProvider
    {
        /// <summary>
        /// 注册聚合根与对应的AggregateRootEvent的类型，一般用户在扩展自己的AggregateRootEvent时需要调用此
        /// 方法注册自己的扩展AggregateRootEvent类型
        /// </summary>
        void RegisterAggregateRootEventTypeMapping(Type aggregateRootType, Type aggregateRootEventType);
        /// <summary>
        /// 返回聚合根对应的AggregateRootEvent的类型
        /// </summary>
        Type GetAggregateRootEventType(Type aggregateRootType);
    }
}
