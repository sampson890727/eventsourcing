//Copyright (c) CodeSharp.  All rights reserved.

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
        /// 将Castle WindsorContainer作为EventSourcing框架的IoC容器
        /// </summary>
        public static Configuration Castle(this Configuration configuration, IWindsorContainer container = null)
        {
            if (container == null)
            {
                container = new WindsorContainer().Install(new ConfigurationInstaller(new XmlInterpreter()));
            }
            configuration.SetResolver(new WindsorContainerResolver(container));

            return configuration;
        }
        /// <summary>
        /// 利用Castle的事务框架实现EventSourcing框架中的TransactionAttribute自动事务提交特性
        /// </summary>
        public static Configuration CastleTransaction(this Configuration configuration)
        {
            var resolver = DependencyResolver.Resolver as WindsorContainerResolver;
            var container = resolver.Container;

            container.Register(Component.For<TransactionInterceptor>().Named("eventsourcing.transaction.interceptor"));
            container.Register(Component.For<TransactionMetaInfoStore>().Named("eventsourcing.transaction.MetaInfoStore"));
            container.Kernel.ComponentModelBuilder.AddContributor(new TransactionComponentInspector());
            container.Register(Component.For<ITransactionManager>().ImplementedBy<TransactionManager>().IsDefault());

            return configuration;
        }
        /// <summary>
        /// 利用Castle的事务框架实现EventSourcing框架中的IUnitOfWorkManager
        /// </summary>
        public static Configuration CastleUnitOfWorkManager(this Configuration configuration)
        {
            DependencyResolver.RegisterType(typeof(UnitOfWorkManager));
            return configuration;
        }
    }
}