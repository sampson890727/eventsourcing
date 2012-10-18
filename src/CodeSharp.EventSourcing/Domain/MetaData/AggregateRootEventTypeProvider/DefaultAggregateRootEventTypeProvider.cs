using System;
using System.Collections.Generic;

namespace CodeSharp.EventSourcing
{
    [Component(LifeStyle.Singleton)]
    public class DefaultAggregateRootEventTypeProvider : IAggregateRootEventTypeProvider
    {
        #region Private Variables

        private readonly Dictionary<Type, Type> _aggregateRootEventTypeDictionary = new Dictionary<Type, Type>();

        #endregion

        #region Public Methods

        /// <summary>
        /// 注册聚合根与对应的AggregateRootEvent的类型，一般用户在扩展自己的AggregateRootEvent时需要调用此
        /// 方法注册自己的扩展AggregateRootEvent类型
        /// </summary>
        public void RegisterAggregateRootEventTypeMapping(Type aggregateRootType, Type aggregateRootEventType)
        {
            Utils.AssertTypeInheritance(aggregateRootType, typeof(AggregateRoot));
            Utils.AssertTypeInheritance(aggregateRootEventType, typeof(AggregateRootEvent));

            if (_aggregateRootEventTypeDictionary.ContainsKey(aggregateRootType))
            {
                throw new EventSourcingException(string.Format("不能为同一个类型的聚合根（Type:{0}）重复注册AggregateRootEvent的类型", aggregateRootType.FullName));
            }

            _aggregateRootEventTypeDictionary.Add(aggregateRootType, aggregateRootEventType);
        }
        /// <summary>
        /// 返回聚合根对应的AggregateRootEvent的类型，如果用户注册了自己的扩展AggregateRootEvent类型，则返回用户自定义的类型；
        /// 否则返回默认的基类AggregateRootEvent泛型类型；
        /// </summary>
        public Type GetAggregateRootEventType(Type aggregateRootType)
        {
            if (_aggregateRootEventTypeDictionary.ContainsKey(aggregateRootType))
            {
                return _aggregateRootEventTypeDictionary[aggregateRootType];
            }
            return typeof(AggregateRootEvent<>).MakeGenericType(aggregateRootType);
        }

        #endregion
    }
}
