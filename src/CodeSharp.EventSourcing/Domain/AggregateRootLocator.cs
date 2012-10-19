//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 内部辅助类，用于获取某个聚合根
    /// </summary>
    internal class AggregateRootLocator
    {
        private IRepository _repository = DependencyResolver.Resolve<IRepository>();
        private static readonly AggregateRootLocator _instance = new AggregateRootLocator();
        private static readonly MethodInfo _internalMethodToGetAggregateRoot = typeof(AggregateRootLocator).GetMethod("InternalGetAggregateRoot", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public static AggregateRoot GetAggregateRoot(Type aggregateRootType, object id, bool getWithLock = false)
        {
            return _internalMethodToGetAggregateRoot.MakeGenericMethod(aggregateRootType).Invoke(_instance, new object[] { id, getWithLock }) as AggregateRoot;
        }

        private TAggregateRoot InternalGetAggregateRoot<TAggregateRoot>(object id, bool getWithLock) where TAggregateRoot : AggregateRoot
        {
            if (getWithLock)
            {
                return _repository.GetByIdWithLock<TAggregateRoot>(id);
            }
            else
            {
                return _repository.GetById<TAggregateRoot>(id);
            }
        }
    }
}
