﻿//Copyright (c) CodeSharp.  All rights reserved.

using System;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 聚合根的快照类定义
    /// </summary>
    public class Snapshot
    {
        #region Constructors

        public Snapshot()
        {
        }
        public Snapshot(Type aggregateRootType, string aggregateRootId, long version, object data, DateTime createdTime)
        {
            AggregateRootType = aggregateRootType;
            AggregateRootId = aggregateRootId;
            Version = version;
            Data = data;
            CreatedTime = createdTime;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// 快照对应的聚合根的类型
        /// </summary>
        public Type AggregateRootType { get; set; }
        /// <summary>
        /// 快照对应的聚合根的类型对应的名称
        /// </summary>
        public string AggregateRootName { get; set; }
        /// <summary>
        /// 快照对应的聚合根的Id
        /// </summary>
        public string AggregateRootId { get; set; }
        /// <summary>
        /// 快照创建时聚合根的版本
        /// </summary>
        public long Version { get; set; }
        /// <summary>
        /// 快照包含的数据的类型对应的名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 快照包含的数据
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 快照包含的数据的字符串形式，默认是json格式
        /// </summary>
        public string SerializedData { get; set; }
        /// <summary>
        /// 快照创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        #endregion

        #region Public Methohds

        /// <summary>
        /// 返回快照的基本数据有效性
        /// </summary>
        public bool IsValid()
        {
            return
                AggregateRootType != null && typeof(AggregateRoot).IsAssignableFrom(AggregateRootType)
                && AggregateRootId != null
                && Version > 0
                && Data != null;
        }
        public override bool Equals(object obj)
        {
            Snapshot snapshot = obj as Snapshot;
            if (snapshot == null)
            {
                return false;
            }
            if (snapshot.AggregateRootName == AggregateRootName &&
                snapshot.AggregateRootId == AggregateRootId &&
                snapshot.Version == Version)
            {
                return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return AggregateRootName.GetHashCode() + AggregateRootId.GetHashCode() + Version.GetHashCode();
        }

        #endregion
    }
}
