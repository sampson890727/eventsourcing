//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections;
using System.Web;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 基于HttpContext实现的UnitOfWork生命周期管理，这种存储表示UnitOfWork的生命周期是
    /// 当前这一次Http请求的整个生命周期。如果当前是Web应用则用该类来存储UnitOfWork的生命周期，
    /// 实现方式同样参考了WebSessionStore的成熟做法。
    /// </summary>
    [Component(LifeStyle.Singleton)]
    public class WebUnitOfWorkStore : AbstractDictStackUnitOfWorkStore
    {
        protected override IDictionary GetDictionary()
        {
            return GetCurrentHttpContext().Items[this.SlotKey] as IDictionary;
        }
        protected override void StoreDictionary(IDictionary dictionary)
        {
            GetCurrentHttpContext().Items[this.SlotKey] = dictionary;
        }

        private static HttpContext GetCurrentHttpContext()
        {
            HttpContext context = HttpContext.Current;

            if (context == null)
            {
                throw new EventSourcingException("WebUnitOfWorkStore: Could not obtain reference to HttpContext");
            }
            return context;
        }
    }
}