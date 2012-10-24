using System;
using System.Reflection;
using CodeSharp.EventSourcing;
using CodeSharp.EventSourcing.MessageBus.Host;

namespace EventSourcing.Sample.MessageBus.Host
{
    class Program
    {
        private static DefaultHost _host;

        static void Main(string[] args)
        {
            var modelAssembly = Assembly.Load("EventSourcing.Sample.Model");
            var applicationAssembly = Assembly.Load("EventSourcing.Sample.Application");
            var entityAssembly = Assembly.Load("EventSourcing.Sample.Entities");
            var mappingAssembly = Assembly.Load("EventSourcing.Sample.Entities.Mappings");
            var eventSubscriberAssembly = Assembly.Load("EventSourcing.Sample.EventSubscribers");
            var assemblies = new Assembly[] { modelAssembly, applicationAssembly, entityAssembly, mappingAssembly, eventSubscriberAssembly };

            _host = new DefaultHost().Start(new StartInfo { ScanningAssemblies = assemblies, DefaultSubscriptionTable = "EventSourcing_Sample_Subscription" });

            DependencyResolver.Resolve<ILoggerFactory>().Create("Program").Info("Host started. Press any key to exit...");
            Console.ReadLine();
        }
    }
}
