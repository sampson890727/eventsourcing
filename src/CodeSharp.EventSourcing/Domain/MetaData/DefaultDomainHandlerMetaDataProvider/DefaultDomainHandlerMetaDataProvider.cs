//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// <see cref="IDomainHandlerMetaDataProvider"/>接口默认实现
    /// </summary>
    [Component(LifeStyle.Singleton)]
    public class DefaultDomainHandlerMetaDataProvider : IDomainHandlerMetaDataProvider
    {
        private readonly MessageHandlerMetaDataManager<DomainHandlerMetaData, DomainHandlerAttribute> _messageHandlerMetaDataManager;

        public DefaultDomainHandlerMetaDataProvider()
        {
            _messageHandlerMetaDataManager = new MessageHandlerMetaDataManager<DomainHandlerMetaData, DomainHandlerAttribute>();
        }

        public IEnumerable<DomainHandlerMetaData> GetMetaDatas(object evnt)
        {
            return _messageHandlerMetaDataManager.GetHandlerMetaDatasForMessage(evnt.GetType());
        }
        public void RegisterAllEventSubscribersInAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var subscriberType in assembly.GetTypes().Where(t => TypeUtils.IsDomainSubscriber(t)))
                {
                    RegisterSubscriber(subscriberType);
                }
            }
        }
        public void RegisterSubscriber(Type subscriberType)
        {
            if (!TypeUtils.IsDomainSubscriber(subscriberType))
            {
                throw new EventSourcingException(
                    "给定的类型‘{0}’不是一个合法的聚合根消息订阅者类型，类型必须继承AggregateRoot且必须至少具有一个标记了DomainHandler特性的方法才可以被注册为订阅者。",
                    subscriberType.FullName);
            }

            _messageHandlerMetaDataManager.RegisterMetaDatasFromType(
                subscriberType,
                (handler, attribute) => new DomainHandlerMetaData
                {
                    Handler = handler,
                    SubscriberType = subscriberType,
                    Paths = attribute.Paths,
                    GetWithLock = attribute.GetWithLock
                });
        }
    }
}
