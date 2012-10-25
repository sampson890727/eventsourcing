using System;
using System.Reflection;
using CodeSharp.EventSourcing.Castles;
using CodeSharp.EventSourcing.NHibernate;

namespace CodeSharp.EventSourcing.MessageBus.Host
{
    /// <summary>
    /// 消息总线宿主默认实现
    /// </summary>
    public class DefaultHost
    {
        public virtual DefaultHost Start(StartInfo startInfo)
        {
            //TODO
            //Configuration.Create(startInfo.EndpointName, null, startInfo.ConfigOutputFileRootPath, startInfo.EntryAssembly, startInfo.ConfigResourceNameSpace)
            //    .Castle()
            //    .Log4Net(startInfo.ConfigOutputFileRootPath + "/" + startInfo.DefaultLog4NetConfigFileName)
            //    .RegisterComponents()
            //    .NHibernate(startInfo.ScanningAssemblies)
            //    .InitAndStartMessageBus(startInfo.EndpointName, startInfo.ScanningAssemblies);

            return this;
        }

        public virtual void Stop()
        {
        }
    }

    public class StartInfo
    {
        public string EndpointName { get; set; }
        public Assembly EntryAssembly { get; set; }
        public Assembly[] ScanningAssemblies { get; set; }
        public string ConfigOutputFileRootPath { get; set; }
        public string DefaultLog4NetConfigFileName { get; set; }
        public string ConfigResourceNameSpace { get; set; }
        public string DefaultEventTable { get; set; }
        public string DefaultSubscriptionTable { get; set; }

        public StartInfo()
        {
            EntryAssembly = Assembly.GetEntryAssembly();
            EndpointName = EntryAssembly.EntryPoint.DeclaringType.Namespace;
            ConfigResourceNameSpace = EntryAssembly.EntryPoint.DeclaringType.Namespace + ".ConfigFiles";
            ConfigOutputFileRootPath = AppDomain.CurrentDomain.BaseDirectory + "application_config";
            DefaultLog4NetConfigFileName = "log4net.config";
        }
    }
}
