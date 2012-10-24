//Copyright (c) CodeSharp.  All rights reserved.

using System.Reflection;

namespace CodeSharp.EventSourcing.Castles
{
    public static class ConfigurationMessageBusHostExtensions
    {
        /// <summary>
        /// 初始化和启动异步消息总线
        /// </summary>
        public static Configuration InitAndStartMessageBus(this Configuration configuration, string endpointName, params Assembly[] assemblies)
        {
            Address.InitializeLocalAddress(endpointName);
            DependencyResolver.RegisterTypes(x => TypeUtils.IsAsyncSubscriber(x), assemblies);
            var messageBus = DependencyResolver.Resolve<IAsyncMessageBus>();
            messageBus.Initialize();
            messageBus.RegisterAllSubscribersInAssemblies(assemblies);
            messageBus.Start();

            return configuration;
        }
    }
}