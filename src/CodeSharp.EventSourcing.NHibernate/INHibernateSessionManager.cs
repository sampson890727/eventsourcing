//Copyright (c) CodeSharp.  All rights reserved.

using NHibernate;

namespace CodeSharp.EventSourcing.NHibernate
{
    public interface INHibernateSessionManager
    {
        /// <summary>
        /// 获取一个当前可用的NHibernate Session
        /// </summary>
        /// <returns></returns>
        ISession OpenSession();
    }
}
