using System;
using System.Reflection;
using CodeSharp.EventSourcing;
using CodeSharp.EventSourcing.Castles;
using CodeSharp.EventSourcing.NHibernate;
using NUnit.Framework;

namespace EventSourcing.Sample.Test
{
    [TestFixture]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class TestBase
    {
        protected Random _random = new Random();
        protected ILogger _logger;
        protected INHibernateSessionManager _sessionManager;

        [TestFixtureSetUp]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
        public void TestFixtureSetUp()
        {
            var modelAssembly = Assembly.Load("EventSourcing.Sample.Model");
            var applicationAssembly = Assembly.Load("EventSourcing.Sample.Application");
            var entityAssembly = Assembly.Load("EventSourcing.Sample.Entities");
            var mappingAssembly = Assembly.Load("EventSourcing.Sample.Entities.Mappings");
            var eventSubscriberAssembly = Assembly.Load("EventSourcing.Sample.EventSubscribers");
            var assemblies = new Assembly[] { modelAssembly, applicationAssembly, entityAssembly, mappingAssembly, eventSubscriberAssembly };

            try
            {
                Configuration.Create("EventSourcing.Sample")
                    .Install(new DefaultConfigurationInstaller(Assembly.GetExecutingAssembly()))
                    .Castle()
                    .CastleTransaction()
                    .CastleUnitOfWorkManager()
                    .Log4Net()
                    .RegisterComponents(assemblies)
                    .NHibernate(assemblies);
            }
            catch (Exception e)
            {
                if (!e.Message.Contains("不可重复初始化框架配置"))
                {
                    Console.WriteLine(e.Message);
                    throw e;
                }
            }

            this._logger = DependencyResolver.Resolve<ILoggerFactory>().Create(this.GetType().Name);
            this._sessionManager = DependencyResolver.Resolve<INHibernateSessionManager>();
        }

        protected string RandomString()
        {
            return "EventSourcing_" + DateTime.Now.ToString("yyyyMMddHHmmss") + DateTime.Now.Ticks + _random.Next(100);
        }
    }
}
