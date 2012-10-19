//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 定义属于核心框架职责的逻辑验证断言
    /// </summary>
    public static class EventSourcingHelper
    {
        /// <summary>
        /// 验证给定的事件是否都属于同一个聚合根, T表示事件所属聚合根的类型
        /// </summary>
        public static void AreEventsBelongtoSameAggregateRoot<T>(IEnumerable<AggregateRootEvent> evnts) where T : AggregateRoot
        {
            if (evnts == null)
            {
                return;
            }

            AggregateRootEvent previousEvent = null;
            foreach (var evnt in evnts)
            {
                if (evnt.AggregateRootType != typeof(T))
                {
                    throw new EventSourcingException(string.Format("检测到要保存的某个事件所属聚合根的类型与要求的聚合根类型不符，事件信息为：({0})，要求的聚合根类型为：({1})", evnt, typeof(T).FullName));
                }
                if (previousEvent != null && previousEvent.AggregateRootId != evnt.AggregateRootId)
                {
                    throw new EventSourcingException(string.Format("检测到要保存的两个事件不属于同一个聚合根，事件信息分别为：({0})，({1})", previousEvent, evnt));
                }
                previousEvent = evnt;
            }
        }
        /// <summary>
        /// 根据给定的聚合根类型及其事件重建一个聚合根对象，该对象只用于显示，不能参与任何业务逻辑；
        /// 所以，该方法不允许在正常业务逻辑的代码中使用，该方法返回的聚合根也不受工作单元监控，只能用于查看聚合根在某个事件版本时的状态；
        /// </summary>
        public static AggregateRoot BuildAggregateRootForViewOnly(Type aggregateRootType, IEnumerable<AggregateRootEvent> evnts)
        {
            if (aggregateRootType == null)
            {
                throw new ArgumentNullException("TAggregateRoot");
            }
            if (evnts == null)
            {
                throw new ArgumentNullException("evnts");
            }

            AggregateRoot aggregateRoot = null;

            if (evnts.Count() > 0)
            {
                aggregateRoot = DependencyResolver.Resolve<IAggregateRootFactory>().CreateAggregateRoot(aggregateRootType);
                aggregateRoot.ReplayEvents(evnts);
            }

            return aggregateRoot;
        }
    }
}
