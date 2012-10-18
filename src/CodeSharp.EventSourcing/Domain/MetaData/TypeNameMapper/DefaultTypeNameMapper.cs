using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 一个类用于维护类型与一个名称的一对一的对应关系
    /// </summary>
    [Component(LifeStyle.Singleton)]
    public class DefaultTypeNameMapper : ITypeNameMapper
    {
        #region Private Variables

        private readonly Dictionary<NameTypeMappingType, Dictionary<string, Type>> _nameTypesDictionary;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DefaultTypeNameMapper()
        {
            _nameTypesDictionary = new Dictionary<NameTypeMappingType, Dictionary<string, Type>>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 根据类型名称返回类型
        /// </summary>
        public Type GetType(NameTypeMappingType mappingType, string name)
        {
            var nameTypes = GetMappingDictionary(mappingType);

            if (!nameTypes.ContainsKey(name))
            {
                var message = string.Format("无法为指定的名称'{0}'找到对应的类型", name);
                throw new ArgumentOutOfRangeException("name", name, message);
            }
            return nameTypes[name];
        }
        /// <summary>
        /// 根据类型返回类型名称
        /// </summary>
        public string GetName(NameTypeMappingType mappingType, Type type)
        {
            var nameTypes = GetMappingDictionary(mappingType);

            if (!nameTypes.ContainsValue(type))
            {
                var message = string.Format("无法为指定的类型'{0}'找到对应的名称", type);
                throw new ArgumentOutOfRangeException("type", type, message);
            }

            return nameTypes.Single(x => x.Value == type).Key;
        }
        /// <summary>
        /// 返回给定的类型是否存在一个对应的名称与之对应
        /// </summary>
        public bool IsTypeExist(NameTypeMappingType mappingType, Type type)
        {
            return GetMappingDictionary(mappingType).ContainsValue(type);
        }
        /// <summary>
        /// 返回给定的名称是否存在一个对应的类型与之对应
        /// </summary>
        public bool IsNameExist(NameTypeMappingType mappingType, string name)
        {
            return GetMappingDictionary(mappingType).ContainsKey(name);
        }
        /// <summary>
        /// 从给定程序集中根据给定的特性扫描所有类型与其名称的映射关系；
        /// </summary>
        public void RegisterAllTypeNameMappings<T>(NameTypeMappingType mappingType, params Assembly[] assemblies) where T : AbstractTypeNameAttribute
        {
            Type typeNameAttributeType = typeof(T);

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(x => x.GetCustomAttributes(typeNameAttributeType, false).Count() > 0))
                {
                    var attribute = type.GetCustomAttributes(typeNameAttributeType, false).Single() as AbstractTypeNameAttribute;
                    RegisterMapping(attribute.MappingType, attribute.Name, type);
                }
            }

            foreach (var assembly in assemblies)
            {
                if (mappingType == NameTypeMappingType.AggregateRootMapping)
                {
                    foreach (var type in assembly.GetTypes().Where(x => TypeUtils.IsAggregateRoot(x)))
                    {
                        if (!IsTypeExist(mappingType, type))
                        {
                            RegisterMapping(mappingType, type.FullName, type);
                        }
                    }
                }
                else if (mappingType == NameTypeMappingType.EventMapping)
                {
                    foreach (var type in assembly.GetTypes().Where(x => TypeUtils.IsEvent(x)))
                    {
                        if (!IsTypeExist(mappingType, type))
                        {
                            RegisterMapping(mappingType, type.FullName, type);
                        }
                    }
                }
                else if (mappingType == NameTypeMappingType.SnapshotMapping)
                {
                    foreach (var type in assembly.GetTypes().Where(x => TypeUtils.IsSnapshot(x)))
                    {
                        if (!IsTypeExist(mappingType, type))
                        {
                            RegisterMapping(mappingType, type.FullName, type);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 为一个类型注册一个名称，一个名称只能对应一个类型
        /// </summary>
        public void RegisterMapping(NameTypeMappingType mappingType, string name, Type type)
        {
            var nameTypes = GetMappingDictionary(mappingType);

            //基本的参数不空的验证
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            //以下验证确保类型与名称之间是一一对应关系
            if (nameTypes.ContainsKey(name))
            {
                Type otherType = nameTypes[name];
                throw new EventSourcingException(string.Format("不能为类型{0}注册名称{1}，因为已经有另外一个类型{2}使用了该名称", type.FullName, name, otherType.FullName));
            }
            if (nameTypes.ContainsValue(type))
            {
                string otherName = nameTypes.Single(x => x.Value == type).Key;
                throw new EventSourcingException(string.Format("不能为名称{0}注册类型{1}，因为已经有另外一个名称{2}使用了该类型", name, type.FullName, otherName));
            }

            nameTypes.Add(name, type);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 根据映射种类获取一个存放了类型与其名称之间映射关系的字典
        /// </summary>
        private Dictionary<string, Type> GetMappingDictionary(NameTypeMappingType mappingType)
        {
            if (!_nameTypesDictionary.ContainsKey(mappingType))
            {
                _nameTypesDictionary.Add(mappingType, new Dictionary<string, Type>());
            }
            return _nameTypesDictionary[mappingType];
        }

        #endregion
    }
}
