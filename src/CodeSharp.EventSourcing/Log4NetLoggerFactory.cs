using System;
using System.IO;
using log4net;
using log4net.Config;

namespace CodeSharp.EventSourcing
{
    public class Log4NetLoggerFactory : ILoggerFactory
    {
        public Log4NetLoggerFactory(string configFile)
        {
            FileInfo file = new FileInfo(configFile);
            XmlConfigurator.ConfigureAndWatch(file);
        }

        ILogger ILoggerFactory.Create(string name)
        {
            return new Log4NetLogger(LogManager.GetLogger(name));
        }
        ILogger ILoggerFactory.Create(Type type)
        {
            return new Log4NetLogger(LogManager.GetLogger(type));
        }
    }
}
