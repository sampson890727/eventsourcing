using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 同步模式的消息总线默认实现
    /// </summary>
    [Component(LifeStyle.Singleton)]
    public class DefaultSyncMessageBus : ISyncMessageBus
    {
        private readonly MessageHandlerMetaDataManager<HandlerMetaData, SyncHandlerAttribute> _messageHandlerMetaDataManager;

        public DefaultSyncMessageBus()
        {
            _messageHandlerMetaDataManager = new MessageHandlerMetaDataManager<HandlerMetaData, SyncHandlerAttribute>();
        }

        void IMessageBus.Initialize() { }

        void IMessageBus.Publish(object message)
        {
            foreach (var metaData in _messageHandlerMetaDataManager.GetHandlerMetaDatasForMessage(message.GetType()))
            {
                var subscriber = DependencyResolver.Resolve(metaData.SubscriberType);
                metaData.Handler.Invoke(subscriber, new object[] { message });
            }
        }
        void IMessageBus.Publish(IEnumerable<object> messages)
        {
            var syncMessageBus = this as ISyncMessageBus;
            foreach (var message in messages)
            {
                syncMessageBus.Publish(message);
            }
        }

        void IMessageBus.RegisterSubscriber<T>()
        {
            var bus = this as ISyncMessageBus;
            bus.RegisterSubscriber(typeof(T));
        }
        void IMessageBus.RegisterSubscriber(Type subscriberType)
        {
            if (!TypeUtils.IsSyncSubscriber(subscriberType))
            {
                throw new EventSourcingException(
                    "类型‘{0}’不是一个有效的消息订阅者，订阅者必须至少具有一个标记了SyncHandler特性的方法。",
                    subscriberType.FullName);
            }

            _messageHandlerMetaDataManager.RegisterMetaDatasFromType(
                subscriberType,
                (handler, attribute) => new HandlerMetaData { Handler = handler, SubscriberType = subscriberType });
        }
        void IMessageBus.RegisterAllSubscribersInAssemblies(params Assembly[] assemblies)
        {
            var bus = this as ISyncMessageBus;
            foreach (var assembly in assemblies)
            {
                foreach (var subscriberType in assembly.GetTypes().Where(t => TypeUtils.IsSyncSubscriber(t)))
                {
                    bus.RegisterSubscriber(subscriberType);
                }
            }
        }

        void IMessageBus.Start() { }
    }
}
