//Copyright (c) CodeSharp.  All rights reserved.

using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace CodeSharp.EventSourcing
{
    public class DefaultSectionHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            var settings = new Dictionary<string, string>();

            foreach (XmlNode childNode in section)
            {
                settings.Add(childNode.Attributes["key"].Value, childNode.Attributes["value"].Value);
            }

            return settings;
        }
    }
}