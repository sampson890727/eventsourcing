using System;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 一个接口用于维护某个类型与其名称之间的一一对应关系
    /// </summary>
    public interface ITypeNameMapper
    {
        /// <summary>
        /// 从给定程序集中根据给定的特性扫描所有类型与其名称的映射关系
        /// </summary>
        void RegisterAllTypeNameMappings<T>(NameTypeMappingType mappingType, params Assembly[] assemblies) where T : AbstractTypeNameAttribute;
        /// <summary>
        /// 根据类型名称返回类型
        /// </summary>
        Type GetType(NameTypeMappingType mappingType, string name);
        /// <summary>
        /// 根据类型返回类型名称
        /// </summary>
        string GetName(NameTypeMappingType mappingType, Type type);
        /// <summary>
        /// 返回给定的类型是否存在一个对应的名称与之对应
        /// </summary>
        bool IsTypeExist(NameTypeMappingType mappingType, Type type);
        /// <summary>
        /// 返回给定的名称是否存在一个对应的类型与之对应
        /// </summary>
        bool IsNameExist(NameTypeMappingType mappingType, string name);
    }
}
