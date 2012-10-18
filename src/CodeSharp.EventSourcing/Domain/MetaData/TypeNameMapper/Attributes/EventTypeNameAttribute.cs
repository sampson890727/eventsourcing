using System;

namespace CodeSharp.EventSourcing
{
    ///<summary>
    /// 一个特性用于指定事件类型的名称
    ///</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EventTypeNameAttribute : AbstractTypeNameAttribute
    {
        public EventTypeNameAttribute(string name)
            : base(name)
        {
        }

        public override NameTypeMappingType MappingType
        {
            get
            {
                return NameTypeMappingType.EventMapping;
            }
        }
    }
}
