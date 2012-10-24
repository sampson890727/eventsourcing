//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using NHibernate;

namespace CodeSharp.EventSourcing.NHibernate
{
    public interface INHibernateDapperQueryService
    {
        IEnumerable<T> Query<T>(string sql, object queryObject);
        T Query<T>(Func<ISession, T> queryFunc);
    }
    public class NHibernateDapperQueryService : INHibernateDapperQueryService
    {
        private INHibernateSessionManager _sessionManager;

        public NHibernateDapperQueryService(INHibernateSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public IEnumerable<T> Query<T>(string sql, object queryObject)
        {
            return Query<IEnumerable<T>>(session => session.Connection.QueryWithNHibernateTransaction<T>(sql, queryObject, session.Transaction));
        }
        public T Query<T>(Func<ISession, T> queryFunc)
        {
            using (var session = _sessionManager.OpenSession())
            {
                return queryFunc(session);
            }
        }
    }
}
