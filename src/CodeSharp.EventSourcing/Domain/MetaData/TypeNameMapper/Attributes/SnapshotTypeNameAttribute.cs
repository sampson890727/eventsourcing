using System;

namespace CodeSharp.EventSourcing
{
    ///<summary>
    /// 一个特性用于指定快照类型的名称
    ///</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SnapshotTypeNameAttribute : AbstractTypeNameAttribute
    {
        public SnapshotTypeNameAttribute(string name)
            : base(name)
        {
        }

        public override NameTypeMappingType MappingType
        {
            get
            {
                return NameTypeMappingType.SnapshotMapping;
            }
        }
    }
}
