using Castle.MicroKernel.Registration;
using Castle.Services.Transaction;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Installer;

namespace CodeSharp.EventSourcing.Castles
{
    public static class ConfigurationCastleExtensions
    {
        /// <summary>
        /// 将一个指定的CastleContainer容器设置为框架需要使用的容器，如果未提供容器，则新建一个容器
        /// </summary>
        public static Configuration Castle(this Configuration configuration, IWindsorContainer container = null)
        {
            if (container == null)
            {
                container = new WindsorContainer().Install(new ConfigurationInstaller(new XmlInterpreter()));
            }
            configuration.SetResolver(new WindsorContainerResolver(container));

            container.Register(Component.For<TransactionInterceptor>().Named("eventsourcing.transaction.interceptor"));
            container.Register(Component.For<TransactionMetaInfoStore>().Named("eventsourcing.transaction.MetaInfoStore"));
            container.Kernel.ComponentModelBuilder.AddContributor(new TransactionComponentInspector());

            container.Register(Component.For<ITransactionManager>().ImplementedBy<TransactionManager>().IsDefault());

            DependencyResolver.RegisterType(typeof(UnitOfWorkManager));

            return configuration;
        }
    }
}