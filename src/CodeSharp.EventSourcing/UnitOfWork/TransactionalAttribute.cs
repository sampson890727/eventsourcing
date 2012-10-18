using System;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 标记一个类中标记了Transaction特性的方法支持事务
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TransactionalAttribute : Attribute
    {
    }
}
