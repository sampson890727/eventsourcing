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
        /// NHibernate相关的所有配置
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static Configuration NHibernate(this Configuration configuration, params Assembly[] assemblies)
        {
            configuration.RegisterNHibernateComponents(assemblies);
            configuration.AddFluentNHibernateMappings(assemblies);
            configuration.CreateAggregateRootEventsDefaultMappingsWithNHibernate(assemblies);
            configuration.RegisterUnitOfWorkStoreAccordingWithNHibernateSessionStore();
            configuration.CreateSubscriptionMappingWithNHibernate();
            return configuration;
        }
        /// <summary>
        /// 注册NHibernate相关的所有组件
        /// </summary>
        public static Configuration RegisterNHibernateComponents(this Configuration configuration, params Assembly[] assemblies)
        {
            DependencyResolver.RegisterType(typeof(NHibernateEventStoreProvider));
            DependencyResolver.RegisterType(typeof(NHibernateSnapshotStoreProvider));
            DependencyResolver.RegisterType(typeof(NHibernateUnitOfWork));
            DependencyResolver.RegisterType(typeof(NHibernateSessionManager));
            DependencyResolver.RegisterType(typeof(NHibernateEventQueryService));
            DependencyResolver.RegisterType(typeof(NHibernateEntityManager));
            DependencyResolver.RegisterType(typeof(NHibernateDapperQueryService));

            return configuration;
        }
        /// <summary>
        /// 根据NHibernateSessionStore的类型注册相应的UnitOfWorkStore
        /// </summary>
        public static Configuration RegisterUnitOfWorkStoreAccordingWithNHibernateSessionStore(this Configuration configuration)
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

            return configuration;
        }
        /// <summary>
        /// 注册给定程序集中的所有的FluentNHibernate ClassMap
        /// </summary>
        public static Configuration AddFluentNHibernateMappings(this Configuration configuration, params Assembly[] assemblies)
        {
            var nhibernateConfiguration = DependencyResolver.Resolve<NHibernateCfg.Configuration>();
            foreach (var assembly in assemblies)
            {
                nhibernateConfiguration.AddMappingsFromAssembly(assembly);
            }

            return configuration;
        }
        /// <summary>
        /// 自动为给定程序集中的所有的AggregateRoot产生的事件与要保存的表通过NHibernate建立ORM映射
        /// </summary>
        public static Configuration CreateAggregateRootEventsDefaultMappingsWithNHibernate(this Configuration configuration, params Assembly[] assemblies)
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
            return configuration;
        }
        /// <summary>
        /// 创建异步事件总线的订阅信息（Subscription）的ORM信息
        /// </summary>
        public static Configuration CreateSubscriptionMappingWithNHibernate(this Configuration configuration)
        {
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
            return configuration;
        }

        private static string GetSettingValue(string key)
        {
            var nhibernateConfiguration = DependencyResolver.Resolve<NHibernateCfg.Configuration>();
            if (nhibernateConfiguration.Properties.ContainsKey(key))
            {
                return nhibernateConfiguration.Properties[key];
            }
            return null;
        }
    }
}