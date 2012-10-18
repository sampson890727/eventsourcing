using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 异步模式消息总线默认实现
    /// </summary>
    [Component(LifeStyle=LifeStyle.Singleton)]
    public class DefaultAsyncMessageBus : IAsyncMessageBus
    {
        #region Private Variables

        private Address _inputAddress;
        private ISubscriptionStorage _subscriptionStorage;
        private IMessageTransport _messageTransport;
        private IMessageSerializer _messageSerializer;
        private int _numberOfWorkerThreads = 2;
        private readonly object _workThreadsLockObj = new object();
        private readonly object _lockObj = new object();
        private readonly MessageHandlerMetaDataManager<HandlerMetaData, AsyncHandlerAttribute> _messageHandlerMetaDataManager;
        private readonly IList<WorkerThread> _workerThreads = new List<WorkerThread>();
        private static ILogger _logger = DependencyResolver.Resolve<ILoggerFactory>().Create("EventSourcing.DefaultAsyncMessageBus");

        #endregion

        /// <summary>
        /// The endpoint address this bus will use as it's message input.
        /// </summary>
        public Address InputAddress
        {
            get
            {
                if (_inputAddress == null)
                {
                    _inputAddress = Address.Local;
                }
                return _inputAddress;
            }
            set
            {
                _inputAddress = value;
            }
        }

        public DefaultAsyncMessageBus(ISubscriptionStorage subscriptionStorage, IMessageTransport messageTransport, IMessageSerializer messageSerializer)
        {
            _messageTransport = messageTransport;
            _subscriptionStorage = subscriptionStorage;
            _messageSerializer = messageSerializer;
            _messageHandlerMetaDataManager = new MessageHandlerMetaDataManager<HandlerMetaData, AsyncHandlerAttribute>();
        }

        void IMessageBus.Initialize()
        {
            _messageTransport.Init(InputAddress);
        }

        void IMessageBus.Publish(object message)
        {
            var addresses = _subscriptionStorage.GetSubscriberAddressesForMessage(message.GetType());
            foreach (var address in addresses)
            {
                var transportMessage = CreateMessage(message);
                _messageTransport.SendMessage(transportMessage, address);
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Sent Message, Id:{0}, Type:{1}, Current Thread Id:{2}", transportMessage.Id, message.GetType().FullName, Thread.CurrentThread.ManagedThreadId);
                }
            }
        }
        void IMessageBus.Publish(IEnumerable<object> messages)
        {
            var asyncMessageBus = this as IAsyncMessageBus;
            foreach (var message in messages)
            {
                asyncMessageBus.Publish(message);
            }
        }

        void IMessageBus.RegisterSubscriber<T>()
        {
            var bus = this as IAsyncMessageBus;
            bus.RegisterSubscriber(typeof(T));
        }
        void IMessageBus.RegisterSubscriber(Type subscriberType)
        {
            if (!TypeUtils.IsAsyncSubscriber(subscriberType))
            {
                throw new EventSourcingException(
                    "类型‘{0}’不是一个有效的异步消息订阅者，订阅者必须至少具有一个标记了AsyncHandler特性的方法。",
                    subscriberType.FullName);
            }

            _messageHandlerMetaDataManager.RegisterMetaDatasFromType(
                subscriberType,
                (handler, attribute) => new HandlerMetaData { Handler = handler, SubscriberType = subscriberType });
        }
        void IMessageBus.RegisterAllSubscribersInAssemblies(params Assembly[] assemblies)
        {
            var bus = this as IAsyncMessageBus;
            foreach (var assembly in assemblies)
            {
                foreach (var subscriberType in assembly.GetTypes().Where(t => TypeUtils.IsAsyncSubscriber(t)))
                {
                    bus.RegisterSubscriber(subscriberType);
                }
            }
        }

        void IMessageBus.Start()
        {
            foreach (var messageType in _messageHandlerMetaDataManager.GetAllMessageTypes())
            {
                SubcribeMessage(messageType);
            }
            for (int index = 0; index < _numberOfWorkerThreads; index++)
            {
                CreateWorkerThread().Start();
            }
        }

        #region Helpers

        private void SubcribeMessage(Type messageType)
        {
            AssertHasInputAddress();
            _subscriptionStorage.Subscribe(InputAddress, messageType);
        }
        private void AssertHasInputAddress()
        {
            if (InputAddress == null)
            {
                throw new EventSourcingException("Please configure the input queue of the async message bus.");
            }
        }
        private Message CreateMessage(object sourceMessage)
        {
            var result = new Message { MessageIntent = MessageIntentEnum.Publish };
            var stream = new MemoryStream();

            _messageSerializer.Serialize(sourceMessage, stream);

            result.Headers = new Dictionary<string, string>();
            result.Headers.Add(TransportHeaderKeys.MessageFullTypeName, sourceMessage.GetType().AssemblyQualifiedName);
            result.ReplyToAddress = InputAddress;
            result.Body = stream.ToArray();
            result.Recoverable = true;
            result.TimeToBeReceived = TimeSpan.MaxValue;

            return result;
        }
        private WorkerThread CreateWorkerThread()
        {
            lock (_workThreadsLockObj)
            {
                var result = new WorkerThread(ReceiveMessage);

                _workerThreads.Add(result);

                result.Stopped += (sender, e) =>
                {
                    var workerThread = sender as WorkerThread;
                    lock (_workThreadsLockObj)
                    {
                        _workerThreads.Remove(workerThread);
                    }
                };

                return result;
            }
        }
        private void ReceiveMessage()
        {
            lock (_lockObj)
            {
                var message = _messageTransport.Receive();
                if (message != null)
                {
                    object rawMessage = _messageSerializer.Deserialize(message);
                    if (rawMessage != null)
                    {
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.DebugFormat("Received Message, Id:{0}, Type:{1}, Current Thread Id:{2}", message.Id, rawMessage.GetType().FullName, Thread.CurrentThread.ManagedThreadId);
                        }
                        DispatchMessageToHandlers(rawMessage);
                    }
                }
            }
        }
        private void DispatchMessageToHandlers(object message)
        {
            foreach (var metaData in _messageHandlerMetaDataManager.GetHandlerMetaDatasForMessage(message.GetType()))
            {
                var subscriber = DependencyResolver.Resolve(metaData.SubscriberType);
                metaData.Handler.Invoke(subscriber, new object[] { message });
            }
        }

        #endregion
    }
}
