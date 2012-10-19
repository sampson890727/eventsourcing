//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// <see cref="IAggregateRootEventCallbackMetaDataProvider"/>接口默认实现类
    /// </summary>
    [Component(LifeStyle.Singleton)]
    public class DefaultAggregateRootEventCallbackMetaDataProvider : IAggregateRootEventCallbackMetaDataProvider
    {
        private Dictionary<CallbackKey, MethodInfo> _callbackMetaDataDict = new Dictionary<CallbackKey, MethodInfo>();
        private ILogger _logger;

        public DefaultAggregateRootEventCallbackMetaDataProvider(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create("EventSourcing.DefaultAggregateRootEventCallbackMetaDataProvider");
        }

        public Action<AggregateRoot, object> GetEventCallback(Type aggregateRootType, Type eventType)
        {
            MethodInfo callbackMethod = null;
            if (_callbackMetaDataDict.TryGetValue(new CallbackKey(aggregateRootType, eventType), out callbackMethod))
            {
                return new Action<AggregateRoot, object>((aggregateRoot, evnt) => callbackMethod.Invoke(aggregateRoot, new object[] { evnt }));
            }
            return null;
        }
        public void RegisterCallbackMetaData(CallbackKey key, MethodInfo method)
        {
            if (_callbackMetaDataDict.ContainsKey(key))
            {
                throw new EventSourcingException(string.Format("聚合根（{0}）上定义了重复的事件（{1}）响应函数。", key.AggregateRootType.FullName, key.EventType.FullName));
            }
            _callbackMetaDataDict.Add(key, method);
            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat(
                    "Cached AggregateRoot Internal Callback Method, AggregateRoot Type:{0}, Event Type:{1}, Method Name:{2}",
                    key.AggregateRootType.FullName,
                    key.EventType.FullName,
                    method.Name
                );
            }
        }

        public void RegisterAllEventCallbackMetaDataInAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => TypeUtils.IsAggregateRoot(t)))
                {
                    var methodNameMatchPattern = "^(on|On|ON|oN)+";
                    var methodEntries = from method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                        let parameters = method.GetParameters()
                                        where
                                           Regex.IsMatch(method.Name, methodNameMatchPattern, RegexOptions.CultureInvariant) &&
                                           parameters.Length == 1
                                        select new { Method = method, EventType = parameters.First().ParameterType };

                    foreach (var methodEntry in methodEntries)
                    {
                        RegisterCallbackMetaData(new CallbackKey(type, methodEntry.EventType), methodEntry.Method);
                    }
                }
            }
        }
    }

    public class CallbackKey
    {
        public CallbackKey(Type aggregateRootType, Type eventType)
        {
            if (aggregateRootType == null)
            {
                throw new ArgumentNullException("TAggregateRoot");
            }
            if (eventType == null)
            {
                throw new ArgumentNullException("eventType");
            }
            AggregateRootType = aggregateRootType;
            EventType = eventType;
        }
        public Type AggregateRootType { get; private set; }
        public Type EventType { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            var comparedKey = obj as CallbackKey;
            return AggregateRootType == comparedKey.AggregateRootType && EventType == comparedKey.EventType;
        }

        public override int GetHashCode()
        {
            return AggregateRootType.GetHashCode() + EventType.GetHashCode();
        }
    }
}
