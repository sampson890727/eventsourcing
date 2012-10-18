using System;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// AggregateRootEvent泛型接口定义，
    /// 该接口允许用户在聚合根内生成AggregateRootEvent时可以返回自己注入的AggregateRootEvent实例
    /// </summary>
    public interface IAggregateRootEvent<T> where T : AggregateRoot
    {
        AggregateRootEvent Initialize(T aggregateRoot, object evnt);
    }
    /// <summary>
    /// 描述聚合根上某个已发生的事件的相关信息
    /// </summary>
    public abstract class AggregateRootEvent
    {
        #region Public Properties

        /// <summary>
        /// 事件所属聚合根的Id
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>
        /// 事件所属聚合根的类型
        /// </summary>
        public Type AggregateRootType { get; set; }
        /// <summary>
        /// 事件所属聚合根的类型对应的名称
        /// </summary>
        public string AggregateRootName { get; set; }
        /// <summary>
        /// 事件的版本号
        /// </summary>
        public long Version { get; set; }
        /// <summary>
        /// 用户定义的事件对象的类型对应的名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 用户定义的事件对象
        /// </summary>
        public object Event { get; set; }
        /// <summary>
        /// 用户定义的事件对象的字符串形式，默认是json格式
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// 事件的发生时间
        /// </summary>
        public DateTime OccurredTime { get; set; }

        #endregion

        #region Public Methods

        public override bool Equals(object obj)
        {
            AggregateRootEvent aggregateRootEvent = obj as AggregateRootEvent;
            if (aggregateRootEvent == null)
            {
                return false;
            }
            if (aggregateRootEvent.AggregateRootName == AggregateRootName &&
                aggregateRootEvent.AggregateRootId == AggregateRootId &&
                aggregateRootEvent.Version == Version)
            {
                return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return AggregateRootName.GetHashCode() + AggregateRootId.GetHashCode() + Version.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("TAggregateRoot:{0}, aggregateRootId: {1}, Event Type:{2}, OccurredTime: {3}",
                                 AggregateRootName == null ? AggregateRootType == null ? null : AggregateRootType.FullName : AggregateRootName,
                                 AggregateRootId,
                                 Event != null ? Event.GetType().FullName : null,
                                 OccurredTime);
        }

        #endregion
    }

    public class AggregateRootEvent<T> : AggregateRootEvent, IAggregateRootEvent<T> where T : AggregateRoot
    {
        #region Public Properties

        /// <summary>
        /// 事件所属聚合根的Id
        /// </summary>
        public new string AggregateRootId
        {
            get
            {
                return base.AggregateRootId;
            }
            set
            {
                base.AggregateRootId = value;
            }
        }
        /// <summary>
        /// 事件所属聚合根的类型对应的名称
        /// </summary>
        public new string AggregateRootName
        {
            get
            {
                return base.AggregateRootName;
            }
            set
            {
                base.AggregateRootName = value;
            }
        }
        /// <summary>
        /// 事件的版本号
        /// </summary>
        public new long Version
        {
            get
            {
                return base.Version;
            }
            set
            {
                base.Version = value;
            }
        }
        /// <summary>
        /// 用户定义的事件对象的类型对应的名称
        /// </summary>
        public new string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }
        /// <summary>
        /// 用户定义的事件对象的字符串形式，默认是json格式
        /// </summary>
        public new string Data
        {
            get
            {
                return base.Data;
            }
            set
            {
                base.Data = value;
            }
        }
        /// <summary>
        /// 事件的发生时间
        /// </summary>
        public new DateTime OccurredTime
        {
            get
            {
                return base.OccurredTime;
            }
            set
            {
                base.OccurredTime = value;
            }
        }

        #endregion

        public virtual AggregateRootEvent Initialize(T aggregateRoot, object evnt)
        {
            AggregateRootId = aggregateRoot.UniqueId;
            AggregateRootType = aggregateRoot.GetType();
            Event = evnt;
            OccurredTime = DateTime.Now;
            return this;
        }
    }
}
