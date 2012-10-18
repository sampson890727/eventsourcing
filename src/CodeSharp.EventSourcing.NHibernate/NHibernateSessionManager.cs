using Castle.Facilities.NHibernateIntegration;
using NHibernate;

namespace CodeSharp.EventSourcing.NHibernate
{
    public class NHibernateSessionManager : INHibernateSessionManager
    {
        private ISessionManager _sessionManager;

        public NHibernateSessionManager(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }
        public ISession OpenSession()
        {
            return _sessionManager.OpenSession();
        }
    }
}