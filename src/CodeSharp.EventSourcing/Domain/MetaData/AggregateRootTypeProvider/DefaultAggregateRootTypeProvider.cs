﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CodeSharp.EventSourcing
{
    [Component(LifeStyle.Singleton)]
    public class DefaultAggregateRootTypeProvider : IAggregateRootTypeProvider
    {
        private List<Type> _aggregateRootTypeList = new List<Type>();

        public IEnumerable<Type> GetAllAggregateRootTypes()
        {
            return _aggregateRootTypeList.AsReadOnly();
        }

        public void RegisterAllAggregateRootTypesInAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                _aggregateRootTypeList.AddRange(assembly.GetTypes().Where(t => TypeUtils.IsAggregateRoot(t)));
            }
        }
    }
}