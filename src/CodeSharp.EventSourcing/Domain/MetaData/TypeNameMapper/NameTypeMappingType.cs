//Copyright (c) CodeSharp.  All rights reserved.

using System;

namespace CodeSharp.EventSourcing
{
    ///<summary>
    /// 表示类型与名称之间映射关系的种类
    ///</summary>
    public enum NameTypeMappingType
    {
        /// <summary>
        /// 聚合根的类型与其名称之间的映射
        /// </summary>
        AggregateRootMapping,
        /// <summary>
        /// 事件的类型与其名称之间的映射
        /// </summary>
        EventMapping,
        /// <summary>
        /// 快照的类型与其名称之间的映射
        /// </summary>
        SnapshotMapping
    }
}
