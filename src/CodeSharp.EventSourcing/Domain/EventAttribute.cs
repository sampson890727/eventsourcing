using System;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 用于标记某个类是一个事件
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class EventAttribute : Attribute
    {
    }
}