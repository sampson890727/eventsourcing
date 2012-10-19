//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// UnitOfWork默认实现
    /// </summary>
    [Transactional]
    public class DefaultUnitOfWork : IUnitOfWork
    {
        private const int MaxTryCount = 100;
        private List<AggregateRoot> _trackingAggregateRoots;
        private ILogger _logger;
        private ISyncMessageBus _eventBus;
        private IDomainHandlerMetaDataProvider _metaDataProvider;

        /// <summary>
        /// 该属性用于当前的UnitOfWork被UnitOfWorkStore存储时，用于存储某个外部对象的引用。
        /// 此设计用于解决并发的情况下获取UnitOfWork可能会不正确的问题。
        /// </summary>
        public object Cookie { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DefaultUnitOfWork(ISyncMessageBus eventBus, ILoggerFactory loggerFactory, IDomainHandlerMetaDataProvider metaDataProvider)
        {
            _eventBus = eventBus;
            _logger = loggerFactory.Create("EventSourcing.DefaultUnitOfWork");
            _trackingAggregateRoots = new List<AggregateRoot>();
            _metaDataProvider = metaDataProvider;
        }

        /// <summary>
        /// 跟踪某个给定的聚合根
        /// </summary>
        public virtual void TrackingAggregateRoot(AggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException("aggregateRoot");
            }
            var existingAggregateRoot = _trackingAggregateRoots.SingleOrDefault(x => x.GetType() == aggregateRoot.GetType() && x.UniqueId == aggregateRoot.UniqueId);
            if (existingAggregateRoot != null)
            {
                _trackingAggregateRoots.Remove(existingAggregateRoot);
            }

            _trackingAggregateRoots.Add(aggregateRoot);
            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Tracked AggregateRoot, Type:{0}, Id:{1}", aggregateRoot.GetType().FullName, aggregateRoot.UniqueId);
            }
        }
        /// <summary>
        /// 返回当前所有被跟踪的聚合根
        /// </summary>
        public virtual IEnumerable<AggregateRoot> GetAllTrackingAggregateRoots()
        {
            return _trackingAggregateRoots.ToList().AsReadOnly();
        }
        /// <summary>
        /// 提交当前所有被跟踪的聚合根上所发生的所有的事件；同时，通知事件总线Publish这些事件。
        /// <remarks>
        /// 在事件总线Publish事件的过程中，各种事件响应者比如Denormalizer会响应这些事件，做出相应的更新。
        /// 目前不建议直接显示调用此方法来提交变更，因为UnitOfWork本身不支持嵌套；
        /// 建议总是在方法上加Transaction特性来实现事务支持。框架会在顶层的Transaction提交前自动调用
        /// UnitOfWork.SubmitChanges方法提交所有事件变更。
        /// </remarks>
        /// </summary>
        [Transaction]
        public virtual IEnumerable<object> SubmitChanges()
        {
            var evnts = FetchUnProcessedEvents();
            if (evnts.Count() > 0)
            {
                //以递归的方式持久化事件以及将事件发回给领域内部
                var totalAggregateRootEvents = new List<AggregateRootEvent>();
                RecursivelyPersistAndPublishEventsToDomain(totalAggregateRootEvents, evnts, 1);

                //发布事件到领域外部
                var totalEvents = totalAggregateRootEvents.Select(evnt => evnt.Event);
                SyncPublishEventsToExternal(totalEvents);

                return totalEvents;
            }
            return new List<object>();
        }
        /// <summary>
        /// 基类默认不做任何事情，子类可以重写此方法来释放一些非托管资源
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// 持久化事件
        /// </summary>
        protected virtual void PersistEvents(IEnumerable<AggregateRootEvent> evnts)
        {
            //先对事件按照聚合根进行分组
            var aggregateRootEventsList = from evnt in evnts
                                        group evnt by new { AggregateRootType = evnt.AggregateRootType, evnt.AggregateRootId } into groupedEvents
                                        select groupedEvents;

            //持久化每个聚合根的事件
            foreach (var aggregateRootEvents in aggregateRootEventsList)
            {
                PersistSingleAggregateRootEvents(aggregateRootEvents.Key.AggregateRootType, aggregateRootEvents);
            }
        }
        /// <summary>
        /// 持久化单个聚合根的事件
        /// </summary>
        protected virtual void PersistSingleAggregateRootEvents(Type aggregateRootType, IEnumerable<AggregateRootEvent> evnts)
        {
            var aggregateRootEventStore = Activator.CreateInstance(typeof(AggregateRootEventStore<>).MakeGenericType(aggregateRootType));
            aggregateRootEventStore.GetType().GetMethod("PersistEvents").Invoke(aggregateRootEventStore, new object[] { evnts });
        }
        /// <summary>
        /// 将领域中产生的事件发回给Domain内部
        /// </summary>
        /// <param name="evnts"></param>
        protected virtual void PublicEventsToDomain(IEnumerable<object> evnts)
        {
            foreach (var evnt in evnts)
            {
                CallAggregateRootEventHandlers(evnt);
            }
        }
        /// <summary>
        /// 通知SyncMessageBus将领域中产生的事件发布到Domain外部
        /// </summary>
        protected virtual void SyncPublishEventsToExternal(IEnumerable<object> evnts)
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Publishing events with {0}, event count:{1}", _eventBus.GetType().Name, evnts.Count());
            }
            _eventBus.Publish(evnts);
            if (_logger.IsDebugEnabled)
            {
                _logger.DebugFormat("Published events with {0}, event count:{1}", _eventBus.GetType().Name, evnts.Count());
            }
        }

        /// <summary>
        /// 该方法通过递归的方式实现
        /// <remarks>
        /// 1）将当前被跟踪的聚合根上发生的所有的事件持久化到EventStore；
        /// 2）将这些事件发布回Domain；之所以要发布回Domain是因为Domain中可能会有一些聚合根会响应这些事件。
        /// 之所以要用递归是因为在将事件发回给Domain并且Domain中有一些聚合根以同步的方式响应这些事件后，
        /// 当前UnitOfWork中又会产生一些新的事件。递归层数最大为100次，超过最大次数，将会抛出异常，整个事务操作也会回滚。
        /// </remarks>
        /// </summary>
        private void RecursivelyPersistAndPublishEventsToDomain(List<AggregateRootEvent> totalAggregateRootEvents, IEnumerable<AggregateRootEvent> unProcessedEvents, int triedCount)
        {
            //如果当前存在需要持久化的事件并且当前重复次数没有超过最大次数，则执行保存事件和发布事件的逻辑
            if (unProcessedEvents.Count() > 0)
            {
                if (triedCount <= MaxTryCount)
                {
                    //持久化事件
                    PersistEvents(unProcessedEvents);
                    //将事件发布回Domain
                    PublicEventsToDomain(unProcessedEvents.Select(evnt => evnt.Event));
                    //将当前已处理的事件进行临时保存，以便最后一次性通过EventBus发布到Domain之外
                    totalAggregateRootEvents.AddRange(unProcessedEvents);
                    //递归尝试下一次调用
                    RecursivelyPersistAndPublishEventsToDomain(totalAggregateRootEvents, FetchUnProcessedEvents(), triedCount + 1);
                }
                else
                {
                    throw new EventSourcingException("RecursivelyPersistAndPublishEventsToDomain递归调用次数超过最大次数100，请检查领域模型内是否有循环事件依赖的问题。");
                }
            }
        }
        /// <summary>
        /// 返回当前被跟踪的所有聚合根上所发生的所有还未被处理的事件
        /// </summary>
        private IEnumerable<AggregateRootEvent> FetchUnProcessedEvents()
        {
            var evnts = new List<AggregateRootEvent>();
            _trackingAggregateRoots.ForEach(x => evnts.AddRange(x.PopEvents()));
            return evnts.AsReadOnly();
        }
        /// <summary>
        /// 调用领域模型内相关聚合根对某个指定事件的响应函数。
        /// </summary>
        private void CallAggregateRootEventHandlers(object evnt)
        {
            var eventHandlerMetaDatas = _metaDataProvider.GetMetaDatas(evnt);
            foreach (var metaData in eventHandlerMetaDatas)
            {
                if (metaData.Paths.Count() == 1)
                {
                    AggregateRootEventHandlerCaller.CallEventHandler(
                        metaData.SubscriberType,
                        metaData.Handler,
                        evnt,
                        metaData.Paths.Single().PropertyName,
                        metaData.GetWithLock);
                }
                else
                {
                    AggregateRootEventHandlerCaller.CallEventHandler(
                        metaData.Handler,
                        evnt,
                        metaData.Paths,
                        metaData.GetWithLock);
                }
            }
        }
        /// <summary>
        /// 内部帮助类，用于避免反射的方式来调用IEventStore泛型接口的StoreEvents方法来持久化单个聚合根的事件。
        /// 这样做的好处是如果以后IEventStore重构时（比如修改了方法名），那么将不会影响这里的代码，因为是强类型的。
        /// </summary>
        private class AggregateRootEventStore<TAggregateRoot> where TAggregateRoot : AggregateRoot
        {
            private readonly static IEventStoreProvider eventStoreProvider = DependencyResolver.Resolve<IEventStoreProvider>();

            public void PersistEvents(IEnumerable<AggregateRootEvent> aggregateRootEvents)
            {
                eventStoreProvider.GetEventStore<TAggregateRoot>().StoreEvents(aggregateRootEvents);
            }
        }
    }
}
