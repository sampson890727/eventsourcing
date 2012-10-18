using System;
using System.Collections.Generic;

namespace CodeSharp.EventSourcing
{
    [Component(LifeStyle.Singleton)]
    public class DefaultSnapshotTypeProvider : ISnapshotTypeProvider
    {
        #region Private Variables

        private readonly Dictionary<Type, Type> _snapshotTypeDictionary = new Dictionary<Type, Type>();

        #endregion

        #region Public Methods

        /// <summary>
        /// 注册聚合根与对应的Snapshot的类型，一般用户在扩展自己的Snapshot时需要调用此
        /// 方法注册自己的扩展Snapshot类型
        /// </summary>
        public void RegisterSnapshotTypeMapping(Type aggregateRootType, Type snapshotType)
        {
            Utils.AssertTypeInheritance(aggregateRootType, typeof(AggregateRoot));
            Utils.AssertTypeInheritance(snapshotType, typeof(Snapshot));

            if (_snapshotTypeDictionary.ContainsKey(aggregateRootType))
            {
                throw new EventSourcingException(string.Format("不能为同一个类型的聚合根（Type:{0}）重复注册Snapshot的类型", aggregateRootType.FullName));
            }

            _snapshotTypeDictionary.Add(aggregateRootType, snapshotType);
        }
        /// <summary>
        /// 返回聚合根对应的Snapshot的类型，如果用户注册了自己的扩展Snapshot类型，则返回用户自定义的类型；
        /// 否则返回默认的基类Snapshot类型；
        /// </summary>
        public Type GetSnapshotType(Type aggregateRootType)
        {
            if (_snapshotTypeDictionary.ContainsKey(aggregateRootType))
            {
                return _snapshotTypeDictionary[aggregateRootType];
            }
            return typeof(Snapshot);
        }

        #endregion
    }
}
