//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// <see cref="IUnitOfWorkStore"/>接口默认实现，
    /// 内部实现逻辑参考了AbstractSessionStore的成熟实现。
    /// </summary>
    public abstract class AbstractUnitOfWorkStore : MarshalByRefObject, IUnitOfWorkStore
    {
        protected abstract Stack GetStackFor(string alias);

        public IUnitOfWork FindCompatibleUnitOfWork(string alias)
        {
            Stack stack = this.GetStackFor(alias);

            if (stack.Count == 0) return null;

            return stack.Peek() as IUnitOfWork;
        }

        public void Store(String alias, IUnitOfWork unitofWork)
        {
            Stack stack = this.GetStackFor(alias);

            stack.Push(unitofWork);

            unitofWork.Cookie = stack;
        }

        public void Remove(IUnitOfWork unitofWork)
        {
            Stack stack = (Stack)unitofWork.Cookie;

            if (stack == null)
            {
                throw new InvalidProgramException("AbstractUnitOfWorkStore.Remove called with no cookie - no pun intended");
            }

            if (stack.Count == 0)
            {
                throw new InvalidProgramException("AbstractUnitOfWorkStore.Remove called for an empty stack");
            }

            IUnitOfWork current = stack.Peek() as IUnitOfWork;

            if (unitofWork != current)
            {
                throw new InvalidProgramException("AbstractUnitOfWorkStore.Remove tried to remove a unitofwork which is not on the top or not in the stack at all");
            }

            stack.Pop();
        }

        public bool IsCurrentActivityEmptyFor(string alias)
        {
            Stack stack = this.GetStackFor(alias);

            return stack.Count == 0;
        }
    }
}