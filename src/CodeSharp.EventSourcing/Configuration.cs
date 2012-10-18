using System;
using System.Collections.Generic;
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
        private string _appName;
        private string _root;
        private string _propertiesFileName = "properties.config";
        private IDictionary<string, ConfigItem> _properties;

        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new NullReferenceException("未初始化框架配置");
                }
                return _instance;
            }
        }
        public string AppName { get { return _appName; } }

        private Configuration(string appName)
        {
            if (string.IsNullOrWhiteSpace(appName))
            {
                throw new ArgumentNullException("appName");
            }
            _appName = appName;
            _properties = new Dictionary<string, ConfigItem>();
        }

        #region Config

        /// <summary>
        /// 初始化框架配置
        /// </summary>
        public static Configuration Config(string appName)
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("不可重复初始化框架配置");
            }
            _instance = new Configuration(appName);
            return _instance;
        }
        /// <summary>从嵌入的xml文件中初始化框架配置
        /// <remarks>
        /// 配置项文件默认为properties.config
        /// </remarks>
        /// </summary>
        public static Configuration ConfigWithEmbeddedXml(string appName, string versionFlag, string folder, Assembly assembly, string nameSpace)
        {
            return Configuration.ConfigWithEmbeddedXml(appName, versionFlag, folder, assembly, nameSpace, null);
        }
        /// <summary>从嵌入的xml文件中初始化框架配置
        /// <remarks>
        /// 配置项文件默认为properties.config
        /// </remarks>
        /// </summary>
        public static Configuration ConfigWithEmbeddedXml(string appName, string versionFlag, string folder, Assembly assembly, string nameSpace, IDictionary<string, string> parameters)
        {
            //初始化配置
            Configuration.Config(appName);
            //配置文件生成路径
            Configuration._instance._root = folder;

            #region 生成配置文件
            assembly.GetManifestResourceNames().ToList().ForEach(o =>
            {
                //可以不使用版本versionFlag
                var prefix = string.IsNullOrEmpty(versionFlag)
                    ? string.Format("{0}.", nameSpace)
                    : string.Format("{0}.{1}.", nameSpace, versionFlag);

                //排除不符合命名空间的文件
                if (o.IndexOf(prefix) < 0)
                {
                    return;
                }
                //properties文件
                if (o.ToLower().IndexOf("properties.config") >= 0)
                {
                    _instance.ReadProperties(assembly, o);
                    return;
                }

                using (var reader = new StreamReader(assembly.GetManifestResourceStream(o)))
                {
                    var content = reader.ReadToEnd();
                    //替换自定义参数
                    if (parameters != null)
                        parameters.ToList().ForEach(p => content = content.Replace(p.Key, p.Value));
                    //写入文件
                    FileHelper.WriteTo(content
                        , Configuration._instance._root
                        , o.Replace(prefix, "")
                        , FileMode.Create);
                }
            });
            #endregion

            return Configuration._instance;
        }

        #endregion

        #region 配置追加 ReadProperties 耦合于properties.config

        /// <summary>从指定程序集中读取配置
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="manifestResourceName">嵌入资源的完整名称</param>
        /// <returns></returns>
        public Configuration ReadProperties(Assembly assembly, string manifestResourceName)
        {
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(manifestResourceName), Encoding.Default))
            {
                return ReadProperties(reader.ReadToEnd());
            }
        }
        /// <summary>从xml文本中读取配置
        /// </summary>
        /// <param name="propertiesXml">配置xml文本，格式参见properties.config</param>
        /// <returns></returns>
        public Configuration ReadProperties(string propertiesXml)
        {
            IList<ConfigItem> configItems;
            return ReadProperties(propertiesXml, out configItems);
        }
        /// <summary>从xml文本中读取配置
        /// <remarks>若出现重复的配置将会覆盖</remarks>
        /// </summary>
        /// <param name="propertiesXml">配置xml文本，格式参见properties.config</param>
        /// <param name="configItems">本次读取的列表</param>
        /// <returns></returns>
        public Configuration ReadProperties(string propertiesXml, out IList<ConfigItem> configItems)
        {
            var body = propertiesXml;
            var items = new List<ConfigItem>();
            var el = XElement.Parse(body).Element("properties");

            el.Descendants()
                .ToList()
                .ForEach(p =>
                {
                    var item = new ConfigItem() { Key = p.Name.LocalName, Value = p.Value };
                    items.Add(item);
                    if (this._properties.ContainsKey(item.Key))
                    {
                        this._properties[item.Key] = item;
                    }
                    else
                    {
                        this._properties.Add(p.Name.LocalName, item);
                    }
                });

            configItems = items;
            return this;
        }
        /// <summary>将所有配置项写入文件
        /// </summary>
        /// <returns></returns>
        public Configuration RenderProperties()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var xml = XElement.Parse(
                  @"<?xml version='1.0' encoding='utf-8' ?>
                    <configuration>
                        <properties></properties> 
                    </configuration>");
                this._properties.ToList().ForEach(o => xml.Element("properties").Add(new XElement(XName.Get(o.Key), o.Value.Value)));
                writer.Write(xml.ToString());
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                FileHelper.WriteTo(stream, _root, _propertiesFileName, FileMode.Create);
            }
            return this;
        }

        #endregion

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
        public Configuration Log4NetLogger(string configFile = "log4net.config")
        {
            DependencyResolver.Register<ILoggerFactory>(new Log4NetLoggerFactory(configFile));
            return this;
        }
        /// <summary>
        /// 注册所有框架运行所需要的默认组件
        /// </summary>
        public Configuration RegisterAllDefaultComponents(params Assembly[] assemblies)
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
            DependencyResolver.RegisterType(typeof(MsmqMessageTransport));
            DependencyResolver.RegisterType(typeof(InMemorySubscriptionStorage));
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
            InitializeAsyncMessageBus(assemblies);

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
        /// <summary>
        /// 初始化异步消息总线
        /// </summary>
        private void InitializeAsyncMessageBus(params Assembly[] assemblies)
        {
            Address.InitializeLocalAddress(Configuration.Instance.AppName);
            DependencyResolver.RegisterTypes(x => TypeUtils.IsAsyncSubscriber(x), assemblies);
            var messageBus = DependencyResolver.Resolve<IAsyncMessageBus>();
            messageBus.Initialize();
            messageBus.RegisterAllSubscribersInAssemblies(assemblies);
            messageBus.Start();
        }

        #endregion

        public class ConfigItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}