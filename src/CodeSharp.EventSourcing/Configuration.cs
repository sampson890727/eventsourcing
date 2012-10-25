//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 用于配置EventSourcing框架的类
    /// </summary>
    public class Configuration
    {
        private static Configuration _instance;
        private string _environment;

        /// <summary>
        /// Singleton单例
        /// </summary>
        public static Configuration Instance { get { return _instance; } }
        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; private set; }
        /// <summary>
        /// 应用的所有配置信息
        /// </summary>
        public IDictionary<string, string> Properties { get; private set; }

        private Configuration(string appName)
        {
            if (string.IsNullOrEmpty(appName))
            {
                throw new ArgumentNullException("appName");
            }

            AppName = appName;
            Properties = new Dictionary<string, string>();
        }

        public static Configuration Create(string appName)
        {
            if (_instance != null)
            {
                throw new EventSourcingException("不可重复初始化框架配置");
            }
            _instance = new Configuration(appName);
            return _instance;
        }

        /// <summary>
        /// 设置当前运行环境，可能的值有：Debug,Test,Release，只能设置一次
        /// </summary>
        /// <param name="environment"></param>
        internal void SetEnvironment(string environment)
        {
            if (_environment != null)
            {
                throw new EventSourcingException("不能重复设置框架运行环境");
            }
            _environment = environment;
        }
        /// <summary>
        /// 通过某个IConfigurationInstaller来对当前Configuration进行设置
        /// </summary>
        /// <param name="installer"></param>
        /// <returns></returns>
        public Configuration Install(IConfigurationInstaller installer)
        {
            installer.Install(this);
            return this;
        }
        /// <summary>
        /// 设置框架需要用到的Ioc容器
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public Configuration SetResolver(IDependencyResolver resolver)
        {
            DependencyResolver.SetResolver(resolver);
            return this;
        }
        /// <summary>
        /// 初始化log4net
        /// </summary>
        public Configuration Log4Net()
        {
            DependencyResolver.Register<ILoggerFactory>(new Log4NetLoggerFactory(Configuration.Instance.Properties["log4netConfigFile"]));
            return this;
        }
        /// <summary>
        /// 注册组件到容器
        /// </summary>
        public Configuration RegisterComponents(params Assembly[] assemblies)
        {
            DependencyResolver.RegisterType(typeof(DefaultAggregateRootTypeProvider));
            DependencyResolver.RegisterType(typeof(DefaultAggregateRootEventCallbackMetaDataProvider));
            DependencyResolver.RegisterType(typeof(DefaultDomainHandlerMetaDataProvider));
            DependencyResolver.RegisterType(typeof(DefaultAggregateRootEventTypeProvider));
            DependencyResolver.RegisterType(typeof(DefaultSnapshotTypeProvider));
            DependencyResolver.RegisterType(typeof(DefaultTypeNameMapper));
            DependencyResolver.RegisterType(typeof(DefaultSyncMessageBus));
            DependencyResolver.RegisterType(typeof(Log4NetLoggerFactory));
            DependencyResolver.RegisterType(typeof(DefaultAggregateRootFactory));
            DependencyResolver.RegisterType(typeof(JsonNetSerializer));
            DependencyResolver.RegisterType(typeof(DefaultSnapshotter));
            DependencyResolver.RegisterType(typeof(NoSnapshotPolicy));
            DependencyResolver.RegisterType(typeof(Repository));
            DependencyResolver.RegisterType(typeof(JsonMessageSerializer));
            DependencyResolver.RegisterType(typeof(InMemorySubscriptionStorage));
            DependencyResolver.RegisterType(typeof(MsmqMessageTransport));
            DependencyResolver.RegisterType(typeof(DefaultAsyncMessageBus));

            DependencyResolver.Resolve<IAggregateRootTypeProvider>().RegisterAllAggregateRootTypesInAssemblies(assemblies);
            DependencyResolver.Resolve<IAggregateRootEventCallbackMetaDataProvider>().RegisterAllEventCallbackMetaDataInAssemblies(assemblies);
            DependencyResolver.Resolve<IDomainHandlerMetaDataProvider>().RegisterAllEventSubscribersInAssemblies(assemblies);

            DependencyResolver.RegisterTypes(TypeUtils.IsComponent, assemblies);
            DependencyResolver.RegisterTypes(TypeUtils.IsRepository, assemblies);
            DependencyResolver.RegisterTypes(TypeUtils.IsService, assemblies);
            DependencyResolver.RegisterTypes(TypeUtils.IsEventStore, assemblies);
            DependencyResolver.RegisterTypes(TypeUtils.IsSnapshotStore, assemblies);

            RegisterTypeNameMappings(assemblies);
            RegisterAggregateRootEvents(assemblies);
            InitializeSyncMessageBus(assemblies);

            return this;
        }

        #region Helper Methods

        /// <summary>
        /// 将所有的聚合根(继承自AggregateRoot基类)、事件(实现了IEvent接口)，以及快照(标记了SnapshotTypeNameAttribute特性的类)与一个指定的名称字符串进行映射关联。
        /// <remarks>
        /// 要指定聚合根的类型的名称，可以通过在类上添加特性：AggregateRootTypeNameAttribute
        /// 要指定事件的类型的名称，可以通过在类上添加特性：EventTypeNameAttribute
        /// 要指定快照的类型的名称，可以通过在类上添加特性：SnapshotTypeNameAttribute
        /// 如果未在需要映射的类型上找到相应的特性，则默认用类型的FullName来表示类型的名称
        /// </remarks>
        /// </summary>
        private void RegisterTypeNameMappings(params Assembly[] assemblies)
        {
            var typeNameMapper = DependencyResolver.Resolve<ITypeNameMapper>();
            typeNameMapper.RegisterAllTypeNameMappings<AggregateRootTypeNameAttribute>(NameTypeMappingType.AggregateRootMapping, assemblies);
            typeNameMapper.RegisterAllTypeNameMappings<EventTypeNameAttribute>(NameTypeMappingType.EventMapping, assemblies);
            typeNameMapper.RegisterAllTypeNameMappings<SnapshotTypeNameAttribute>(NameTypeMappingType.SnapshotMapping, assemblies);
        }
        /// <summary>
        /// 注册给定程序集中的所有的AggregateRootEvent，扫描实现了IAggregateRootEvent接口的类
        /// </summary>
        private void RegisterAggregateRootEvents(params Assembly[] assemblies)
        {
            DependencyResolver.RegisterTypes(TypeUtils.IsAggregateRootEvent, assemblies);
            var provider = DependencyResolver.Resolve<IAggregateRootEventTypeProvider>();
            foreach (var assembly in assemblies)
            {
                foreach (var aggregateRootEventType in assembly.GetTypes().Where(x => TypeUtils.IsAggregateRootEvent(x)))
                {
                    provider.RegisterAggregateRootEventTypeMapping(TypeUtils.GetAggregateRootType(aggregateRootEventType), aggregateRootEventType);
                }
            }
        }
        /// <summary>
        /// 初始化同步消息总线
        /// </summary>
        private void InitializeSyncMessageBus(params Assembly[] assemblies)
        {
            DependencyResolver.RegisterTypes(x => TypeUtils.IsSyncSubscriber(x), assemblies);
            var messageBus = DependencyResolver.Resolve<ISyncMessageBus>();
            messageBus.Initialize();
            messageBus.RegisterAllSubscribersInAssemblies(assemblies);
            messageBus.Start();
        }

        #endregion
    }
}