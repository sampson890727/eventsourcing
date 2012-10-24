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
    public class DefaultConfigurationInstaller : IConfigurationInstaller
    {
        protected IList<string> _supportedEnvironments;
        protected string _environment;
        protected Assembly _configFileAssembly;
        protected string _configFilePrefix;
        protected string _configFileTargetFolder;
        protected string _configFile;

        public DefaultConfigurationInstaller(Assembly configFileAssembly)
        {
            _supportedEnvironments = new List<string> { "Debug", "Test", "Release" };
            _environment = "Debug";
            _configFileAssembly = configFileAssembly;
            _configFile = "eventsourcing.config";
            _configFilePrefix = configFileAssembly.GetName().Name + ".ConfigFiles";
            _configFileTargetFolder = "application_config";
        }

        public virtual void Install(Configuration configuration)
        {
            GetSectionSettings();
            SetConfigurationEnvironment(configuration);
            SetConfigurationProperites(configuration);
            WriteManifestResourceToFiles();
        }

        protected virtual void GetSectionSettings()
        {
            var settings = ConfigurationManager.GetSection("eventsourcing") as IDictionary<string, string>;

            var environment = settings["environment"];
            if (!string.IsNullOrEmpty(environment))
            {
                if (!_supportedEnvironments.Contains(environment))
                {
                    throw new EventSourcingException("无效的环境配置值，必须为：Debug,Test,Release其中之一");
                }
                _environment = environment;
            }

            var configFile = settings["configFile"];
            if (!string.IsNullOrEmpty(configFile))
            {
                _configFile = configFile;
            }

            var configFilePrefix = settings["configFilePrefix"];
            if (!string.IsNullOrEmpty(configFilePrefix))
            {
                _configFilePrefix = configFilePrefix;
            }

            var configFileTargetFolder = settings["configFileTargetFolder"];
            if (!string.IsNullOrEmpty(configFileTargetFolder))
            {
                _configFileTargetFolder = configFileTargetFolder;
            }
        }
        protected virtual void SetConfigurationEnvironment(Configuration configuration)
        {
            configuration.SetEnvironment(_environment);
        }
        protected virtual void SetConfigurationProperites(Configuration configuration)
        {
            var configFileResourceName = string.Format("{0}.{1}.{2}", _configFilePrefix, _environment, _configFile);
            using (var reader = new StreamReader(_configFileAssembly.GetManifestResourceStream(configFileResourceName), Encoding.UTF8))
            {
                var content = reader.ReadToEnd();
                var element = XElement.Parse(content).Element("properties");

                foreach (var child in element.Descendants())
                {
                    var key = child.Attribute("key").Value;
                    var value = child.Attribute("value").Value;

                    if (configuration.Properties.ContainsKey(key))
                    {
                        configuration.Properties[key] = value;
                    }
                    else
                    {
                        configuration.Properties.Add(key, value);
                    }
                }
            }
        }
        protected virtual void WriteManifestResourceToFiles()
        {
            var prefix = string.Format("{0}.", _configFilePrefix);
            var prefix_WithCurrentEnvironment = string.Format("{0}.{1}.", _configFilePrefix, _environment);
            var targetPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configFileTargetFolder);

            foreach (var resourceName in _configFileAssembly.GetManifestResourceNames())
            {
                //输出所有与环境无关的所有配置文件
                if (resourceName.IndexOf(prefix) >= 0 && _supportedEnvironments.All(x => !resourceName.Contains(string.Format("{0}.{1}.", _configFilePrefix, x))))
                {
                    using (var reader = new StreamReader(_configFileAssembly.GetManifestResourceStream(resourceName)))
                    {
                        SaveTextToFile(reader.ReadToEnd(), targetPath, resourceName.Replace(prefix, ""));
                    }
                }
                //输出当前环境下的所有配置文件
                if (resourceName.IndexOf(prefix_WithCurrentEnvironment) >= 0)
                {
                    using (var reader = new StreamReader(_configFileAssembly.GetManifestResourceStream(resourceName)))
                    {
                        SaveTextToFile(reader.ReadToEnd(), targetPath, resourceName.Replace(prefix_WithCurrentEnvironment, ""));
                    }
                }
            }
        }
        private void SaveTextToFile(string text, string path, string fileName)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (var writer = new StreamWriter(System.IO.Path.Combine(path, fileName), false, Encoding.UTF8))
            {
                writer.Write(text ?? string.Empty);
            }
        }
    }
}