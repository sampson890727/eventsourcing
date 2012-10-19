//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 基于字典和堆栈存储实现的UnitOfWorkStore抽象基类，
    /// 实现逻辑参考了AbstractDictStackSessionStore的成熟做法
    /// </summary>
    public abstract class AbstractDictStackUnitOfWorkStore : AbstractUnitOfWorkStore
    {
        private string slotKey;

        protected String SlotKey
        {
            get
            {
                if (string.IsNullOrEmpty(this.slotKey))
                {
                    this.slotKey = string.Format("eventsourcing.facility.stacks.{0}", Guid.NewGuid());
                }
                return this.slotKey;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override Stack GetStackFor(string alias)
        {
            if (alias == null) throw new ArgumentNullException("alias");
            IDictionary stackDictionary = this.GetDictionary();

            if (stackDictionary == null)
            {
                stackDictionary = new HybridDictionary(true);
                StoreDictionary(stackDictionary);
            }

            Stack stack = stackDictionary[alias] as Stack;

            if (stack == null)
            {
                stack = Stack.Synchronized(new Stack());
                stackDictionary[alias] = stack;
            }

            return stack;
        }

        protected abstract IDictionary GetDictionary();
        protected abstract void StoreDictionary(IDictionary dictionary);
    }
}