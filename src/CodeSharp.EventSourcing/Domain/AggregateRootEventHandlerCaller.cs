//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 内部辅助类，用于通知聚合根响应事件
    /// </summary>
    internal class AggregateRootEventHandlerCaller
    {
        public static void CallEventHandler(Type aggregateRootType, MethodInfo eventHandler, object evnt, string propertyName, bool getWithLock)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }

            var eventType = evnt.GetType();
            var propertyInfo = eventType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new EventSourcingException(string.Format("Property '{0}' not exist in event type '{1}'", propertyName, eventType.FullName));
            }

            var aggregateRootId = propertyInfo.GetValue(evnt, null);
            if (aggregateRootId != null)
            {
                var logger = DependencyResolver.Resolve<ILoggerFactory>().Create("EventSourcing.AggregateRootEventHandlerCaller");
                var aggregateRoot = AggregateRootLocator.GetAggregateRoot(aggregateRootType, aggregateRootId, getWithLock);
                if (aggregateRoot != null)
                {
                    if (logger.IsDebugEnabled)
                    {
                        logger.DebugFormat("Invoking AggregateRoot {0} EventHandler, event type:{1}.", aggregateRootType.Name, eventType.Name);
                    }
                    eventHandler.Invoke(aggregateRoot, new object[] { evnt });
                    if (logger.IsDebugEnabled)
                    {
                        logger.DebugFormat("Invoked AggregateRoot {0} EventHandler, event type:{1}.", aggregateRootType.Name, eventType.Name);
                    }
                }
                else
                {
                    logger.ErrorFormat(
                        "Could not find the aggregate root as a subscriber of event, Please verify whether it was deleted from the database. AggregateRoot Id:{0}, Event Type:{1}, AggregateRootId PropertyName:{2}",
                        aggregateRootId,
                        eventType.FullName,
                        propertyName
                    );
                }
            }
        }
        public static void CallEventHandler(MethodInfo eventHandler, object evnt, IEnumerable<Path> paths, bool getWithLock)
        {
            if (paths == null || paths.Count() == 0) return;

            var logger = DependencyResolver.Resolve<ILoggerFactory>().Create("EventSourcing.AggregateRootEventHandlerCaller");
            var eventType = evnt.GetType();
            var sourceObject = evnt;
            AggregateRoot aggregateRoot = null;

            for(var index = 0; index < paths.Count(); index++)
            {
                var path = paths.ElementAt(index);
                if (index == paths.Count() - 1)
                {
                    aggregateRoot = GetAggregateRootFromProperty(path.AggregateRootType, sourceObject, path.PropertyName, getWithLock);
                }
                else
                {
                    aggregateRoot = GetAggregateRootFromProperty(path.AggregateRootType, sourceObject, path.PropertyName, false);
                }

                if (aggregateRoot == null)
                {
                    break;
                }
                else
                {
                    sourceObject = aggregateRoot;
                }
            }

            if (aggregateRoot != null)
            {
                if (logger.IsDebugEnabled)
                {
                    logger.DebugFormat("Invoking AggregateRoot {0} EventHandler, event type:{1}.", aggregateRoot.GetType().Name, eventType.Name);
                }
                eventHandler.Invoke(aggregateRoot, new object[] { evnt });
                if (logger.IsDebugEnabled)
                {
                    logger.DebugFormat("Invoked AggregateRoot {0} EventHandler, event type:{1}.", aggregateRoot.GetType().Name, eventType.Name);
                }
            }
            else
            {
                logger.ErrorFormat(
                    "Could not find the aggregate root as a subscriber of event, Please verify whether it was deleted from the database. Event Type:{0}, Paths:{1}",
                    eventType.FullName,
                    string.Join("->", paths.Select(x => string.Format("Type:{0},PropertyName:{1}", x.AggregateRootType.Name, x.PropertyName)).ToArray())
                );
            }
        }

        private static AggregateRoot GetAggregateRootFromProperty(Type aggregateRootType, object sourceObject, string propertyName, bool getWithLock)
        {
            if (aggregateRootType == null || sourceObject == null || string.IsNullOrEmpty(propertyName)) return null;

            var propertyInfo = sourceObject.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new EventSourcingException(string.Format("Property '{0}' not exist in object type '{1}'", propertyName, sourceObject.GetType().FullName));
            }
            var aggregateRootId = propertyInfo.GetValue(sourceObject, null);
            if (aggregateRootId != null)
            {
                return AggregateRootLocator.GetAggregateRoot(aggregateRootType, aggregateRootId, getWithLock);
            }

            return null;
        }
    }
}
