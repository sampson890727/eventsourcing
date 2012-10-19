//Copyright (c) CodeSharp.  All rights reserved.

using System.Collections;
using System.Runtime.Remoting.Messaging;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 基于CallContext实现的UnitOfWork生命周期管理，这种存储表示UnitOfWork的生命周期是
    /// 当前逻辑线程的调用上下文。如果当前是非Web应用则用该类来存储UnitOfWork的生命周期，
    /// 实现方式同样参考了CallContextSessionStore的成熟做法。
    /// </summary>
    [Component(LifeStyle.Singleton)]
    public class CallContextUnitOfWorkStore : AbstractDictStackUnitOfWorkStore
    {
        protected override IDictionary GetDictionary()
        {
            return CallContext.GetData(this.SlotKey) as IDictionary;
        }
        protected override void StoreDictionary(IDictionary dictionary)
        {
            CallContext.SetData(this.SlotKey, dictionary);
        }
    }
}