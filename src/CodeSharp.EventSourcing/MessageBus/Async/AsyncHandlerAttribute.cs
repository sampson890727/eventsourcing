using System;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 用于标记某个方法是某个消息的异步响应函数
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AsyncHandlerAttribute : Attribute
    {
    }
}