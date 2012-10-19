//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 一个辅助类，用于管理消息订阅者与消息元数据的映射信息
    /// </summary>
    /// <typeparam name="TMetaData">元数据类型</typeparam>
    /// <typeparam name="THandlerAttribute">某个自定义特性，针对标记了该特性的方法提取元数据信息</typeparam>
    public class MessageHandlerMetaDataManager<TMetaData, THandlerAttribute>
        where TMetaData : class
        where THandlerAttribute : Attribute
    {
        private readonly Dictionary<Type, List<TMetaData>> _metaDataDictionary = new Dictionary<Type, List<TMetaData>>();

        public void RegisterMetaDatasFromType(Type subscriberType, Func<MethodInfo, THandlerAttribute, TMetaData> createMeta)
        {
            foreach (var handler in TypeUtils.GetMethods<THandlerAttribute>(subscriberType))
            {
                var attribute = TypeUtils.GetMethodAttribute<THandlerAttribute>(handler);
                var messageType = handler.GetParameters().First().ParameterType;
                var metaData = createMeta(handler, attribute);
                List<TMetaData> metaDataList = null;
                if (!_metaDataDictionary.TryGetValue(messageType, out metaDataList))
                {
                    metaDataList = new List<TMetaData>();
                    _metaDataDictionary.Add(messageType, metaDataList);
                }
                metaDataList.Add(metaData);
            }
        }
        public IEnumerable<Type> GetAllMessageTypes()
        {
            return _metaDataDictionary.Keys;
        }
        public IEnumerable<TMetaData> GetHandlerMetaDatasForMessage(Type messageType)
        {
            var metaDatas = new List<TMetaData>();
            foreach (var key in _metaDataDictionary.Keys)
            {
                if (key.IsAssignableFrom(messageType))
                {
                    metaDatas.AddRange(_metaDataDictionary[key]);
                }
            }

            return metaDatas;
        }
    }
}
