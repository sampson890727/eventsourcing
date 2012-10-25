//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Linq;
using System.Reflection;
using Castle.Facilities.NHibernateIntegration;
using Castle.Facilities.NHibernateIntegration.SessionStores;
using FluentNHibernate;
using NHibernate.Mapping.ByCode;
using NHibernateCfg = NHibernate.Cfg;

namespace CodeSharp.EventSourcing.NHibernate
{
    public static class ConfigurationNHibernateExtensions
    {
        /// <summary>
        /// 将NHibernate作为EventSourcing框架的持久化框架
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static Configuration NHibernate(this Configuration configuration, params Assembly[] assemblies)
        {
            RegisterNHibernateComponents(assemblies);
            AddFluentNHibernateMappings(assemblies);
            CreateAggregateRootEventsDefaultMappingsWithNHibernate(assemblies);
            RegisterUnitOfWorkStoreAccordingWithNHibernateSessionStore();
            CreateSubscriptionMappingWithNHibernate();
            return configuration;
        }

        /// <summary>
        /// 注册基于NHibernate实现的所有相关组件
        /// </summary>
        private static void RegisterNHibernateComponents(params Assembly[] assemblies)
        {
            DependencyResolver.RegisterType(typeof(NHibernateEventStoreProvider));
            DependencyResolver.RegisterType(typeof(NHibernateSnapshotStoreProvider));
            DependencyResolver.RegisterType(typeof(NHibernateUnitOfWork));
            DependencyResolver.RegisterType(typeof(NHibernateSessionManager));
            DependencyResolver.RegisterType(typeof(NHibernateEventQueryService));
            DependencyResolver.RegisterType(typeof(NHibernateEntityManager));
            DependencyResolver.RegisterType(typeof(NHibernateDapperQueryService));
        }
        /// <summary>
        /// 根据NHibernateSessionStore的类型注册相应的UnitOfWorkStore
        /// </summary>
        private static void RegisterUnitOfWorkStoreAccordingWithNHibernateSessionStore()
        {
            var sessionStoreType = DependencyResolver.Resolve<ISessionStore>().GetType();
            if (sessionStoreType == typeof(WebSessionStore))
            {
                DependencyResolver.RegisterType(typeof(WebUnitOfWorkStore));
            }
            else
            {
                DependencyResolver.RegisterType(typeof(CallContextUnitOfWorkStore));
            }
        }
        /// <summary>
        /// 注册给定程序集中的所有的FluentNHibernate ClassMap
        /// </summary>
        private static void AddFluentNHibernateMappings(params Assembly[] assemblies)
        {
            var nhibernateConfiguration = DependencyResolver.Resolve<NHibernateCfg.Configuration>();
            foreach (var assembly in assemblies)
            {
                nhibernateConfiguration.AddMappingsFromAssembly(assembly);
            }
        }
        /// <summary>
        /// 自动为给定程序集中的所有的AggregateRoot产生的事件与要保存的表通过NHibernate建立ORM映射
        /// </summary>
        private static void CreateAggregateRootEventsDefaultMappingsWithNHibernate(params Assembly[] assemblies)
        {
            var defaultEventTable = Configuration.Instance.Properties["defaultEventTable"];
            if (!string.IsNullOrEmpty(defaultEventTable))
            {
                var nhibernateConfiguration = DependencyResolver.Resolve<NHibernateCfg.Configuration>();
                foreach (var assembly in assemblies)
                {
                    var mapper = new ModelMapper();
                    foreach (var type in assembly.GetTypes().Where(x => TypeUtils.IsAggregateRoot(x)))
                    {
                        var mappingType = typeof(AggregateRootEventMapping<>).MakeGenericType(type);
                        var mapping = Activator.CreateInstance(mappingType, defaultEventTable) as IConformistHoldersProvider;
                        mapper.AddMapping(mapping);
                    }
                    var hbmMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
                    nhibernateConfiguration.AddMapping(hbmMapping);
                }
            }
        }
        /// <summary>
        /// 创建异步事件总线的订阅信息（Subscription）的ORM信息
        /// </summary>
        private static void CreateSubscriptionMappingWithNHibernate()
        {
            //TODO
            //var defaultSubscriptionTable = Configuration.Instance.Properties["defaultSubscriptionTable"];
            //if (!string.IsNullOrEmpty(defaultSubscriptionTable))
            //{
            //    var nhibernateConfiguration = DependencyResolver.Resolve<NHibernateCfg.Configuration>();

            //    var mapper = new ModelMapper();
            //    var mapping = Activator.CreateInstance(typeof(Subscription), defaultSubscriptionTable) as IConformistHoldersProvider;
            //    mapper.AddMapping(mapping);

            //    var hbmMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
            //    nhibernateConfiguration.AddMapping(hbmMapping);
            //}
        }
    }
}