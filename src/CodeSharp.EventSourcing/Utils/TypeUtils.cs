using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 工具类，提供各种工具方法
    /// </summary>
    public sealed class TypeUtils
    {
        /// <summary>判断是否是MVC Controller
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsController(Type type)
        {
            return type != null
                   && type.Name.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase)
                   && !type.IsAbstract;
        }
        /// <summary>判断是否是Repository
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsRepository(Type type)
        {
            return type != null
                 && type.Name.EndsWith("Repository", StringComparison.InvariantCultureIgnoreCase)
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>判断是否是Service
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsService(Type type)
        {
            return type != null
                 && type.Name.EndsWith("Service", StringComparison.InvariantCultureIgnoreCase)
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>判断是否有ComponentAttribute属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsComponent(Type type)
        {
            return type != null
                 && type.GetCustomAttributes(typeof(ComponentAttribute), false).Count() > 0
                 && !type.IsAbstract
                 && !type.IsInterface;
        }
        /// <summary>
        /// 判断某个类型是否是聚合根
        /// </summary>
        /// <returns></returns>
        public static bool IsAggregateRoot(Type type)
        {
            return type.IsClass
                && !type.IsAbstract
                && typeof(AggregateRoot).IsAssignableFrom(type);
        }
        public static bool IsEvent(Type type)
        {
            return type.IsClass
                && !type.IsAbstract
                && !type.IsGenericType
                && !type.IsGenericTypeDefinition
                && type.GetCustomAttributes(typeof(EventAttribute), false).Count() > 0;
        }
        public static bool IsSnapshot(Type type)
        {
            return type.IsClass
                && !type.IsAbstract
                && typeof(ISnapshot).IsAssignableFrom(type);
        }
        /// <summary>
        /// 返回一个类是否是一个EventStore类
        /// </summary>
        public static bool IsEventStore(Type type)
        {
            return type != null
                && type.IsClass
                && type.Name.EndsWith("EventStore", StringComparison.OrdinalIgnoreCase)
                && !type.IsAbstract
                && type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventStore<>));
        }
        /// <summary>
        /// 返回一个类是否是一个SnapshotStore类
        /// </summary>
        public static bool IsSnapshotStore(Type type)
        {
            return type != null
                && type.IsClass
                && type.Name.EndsWith("SnapshotStore", StringComparison.OrdinalIgnoreCase)
                && !type.IsAbstract
                && type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISnapshotStore<>));
        }

        /// <summary>
        /// 返回一个类是否是一个聚合根且至是一个领域内的事件订阅者
        /// </summary>
        public static bool IsDomainSubscriber(Type type)
        {
            return type != null && type.IsClass && !type.IsAbstract && IsAggregateRoot(type) && GetMethods<DomainHandlerAttribute>(type).Count() > 0;
        }
        /// <summary>
        /// 返回一个类是否是一个同步的事件订阅者
        /// </summary>
        public static bool IsSyncSubscriber(Type type)
        {
            return type != null && type.IsClass && !type.IsAbstract && GetMethods<SyncHandlerAttribute>(type).Count() > 0;
        }
        /// <summary>
        /// 返回一个类是否是一个异步的事件订阅者
        /// </summary>
        public static bool IsAsyncSubscriber(Type type)
        {
            return type != null && type.IsClass && !type.IsAbstract && GetMethods<AsyncHandlerAttribute>(type).Count() > 0;
        }

        /// <summary>
        /// 返回指定objectType中标记了T自定义特性的所有方法
        /// </summary>
        public static IEnumerable<MethodInfo> GetMethods<T>(Type objectType) where T : Attribute
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            return objectType.GetMethods(flags).Where(x => x.GetCustomAttributes(typeof(T), false).Count() == 1);
        }
        /// <summary>
        /// 返回指定方法的类型为T的特性实例
        /// </summary>
        public static T GetMethodAttribute<T>(MethodInfo method) where T : Attribute
        {
            return method.GetCustomAttributes(typeof(T), false).Single() as T;
        }



        /// <summary>
        /// 返回一个类是否是一个AggregateRootEvent类
        /// </summary>
        public static bool IsAggregateRootEvent(Type type)
        {
            return type != null
                && type.IsClass
                && type.Name.EndsWith("AggregateRootEvent", StringComparison.OrdinalIgnoreCase)
                && !type.IsAbstract
                && type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAggregateRootEvent<>));
        }
        /// <summary>
        /// 从IAggregateRootEvent接口获取对应聚合根的类型
        /// </summary>
        public static Type GetAggregateRootType(Type aggregateRootEventType)
        {
            return aggregateRootEventType
                    .GetInterfaces()
                    .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAggregateRootEvent<>))
                    .GetGenericArguments()[0];
        }
    }
}