using System;

namespace CodeSharp.EventSourcing
{
    ///<summary>
    /// 一个特性用于指定聚合根类型的名称
    ///</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AggregateRootTypeNameAttribute : AbstractTypeNameAttribute
    {
        public AggregateRootTypeNameAttribute(string name)
            : base(name)
        {
        }

        public override NameTypeMappingType MappingType
        {
            get
            {
                return NameTypeMappingType.AggregateRootMapping;
            }
        }
    }
}
