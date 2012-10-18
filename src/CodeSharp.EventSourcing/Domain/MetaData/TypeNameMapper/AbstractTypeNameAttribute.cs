using System;

namespace CodeSharp.EventSourcing
{
    ///<summary>
    /// 一个特性用于指定某个类型的名称
    ///</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public abstract class AbstractTypeNameAttribute : Attribute
    {
        public AbstractTypeNameAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            Name = name;
        }

        /// <summary>
        /// 类型的名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 映射的种类
        /// </summary>
        public abstract NameTypeMappingType MappingType { get; }
    }
}
