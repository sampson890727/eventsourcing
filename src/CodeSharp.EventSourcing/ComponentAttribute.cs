using System;

namespace CodeSharp.EventSourcing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        public LifeStyle LifeStyle { get; set; }
        public ComponentAttribute() : this(LifeStyle.Transient) { }
        public ComponentAttribute(LifeStyle lifeStyle)
        {
            this.LifeStyle = lifeStyle;
        }
    }
    /// <summary>
    /// 组件生命周期
    /// </summary>
    public enum LifeStyle
    {
        Transient = 0,
        Singleton
    }
}