//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 一个接口，可以提供当前应用内所有聚合根的类型
    /// </summary>
    public interface IAggregateRootTypeProvider
    {
        /// <summary>
        /// 返回当前应用内所有聚合根的类型
        /// </summary>
        IEnumerable<Type> GetAllAggregateRootTypes();
        /// <summary>
        /// 注册所有的聚合根类型
        /// </summary>
        /// <param name="assemblies"></param>
        void RegisterAllAggregateRootTypesInAssemblies(params Assembly[] assemblies);
    }
}
