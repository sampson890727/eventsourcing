//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Castle.Core.Configuration;

namespace CodeSharp.EventSourcing.Castles
{
    public class TransactionMetaInfoStore : MarshalByRefObject
    {
        private readonly IDictionary _type2MetaInfo = new HybridDictionary();

        #region MarshalByRefObject overrides

        /// <summary>
        /// Overrides the MBRO Lifetime initialization
        /// </summary>
        /// <returns>Null</returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        public TransactionMetaInfo CreateMetaFromType(Type implementation)
        {
            TransactionMetaInfo metaInfo = new TransactionMetaInfo();

            PopulateMetaInfoFromType(metaInfo, implementation);

            Register(implementation, metaInfo);

            return metaInfo;
        }
        public TransactionMetaInfo CreateMetaFromConfig(Type implementation, IList<MethodInfo> methods, IConfiguration config)
        {
            TransactionMetaInfo metaInfo = GetMetaFor(implementation);

            if (metaInfo == null)
            {
                metaInfo = new TransactionMetaInfo();
            }

            foreach (MethodInfo method in methods)
            {
                metaInfo.Add(method, new TransactionAttribute());
            }

            Register(implementation, metaInfo);

            return metaInfo;
        }
        public TransactionMetaInfo GetMetaFor(Type implementation)
        {
            return (TransactionMetaInfo)_type2MetaInfo[implementation];
        }

        private static void PopulateMetaInfoFromType(TransactionMetaInfo metaInfo, Type implementation)
        {
            if (implementation == typeof(object) || implementation == typeof(MarshalByRefObject)) return;

            MethodInfo[] methods = implementation.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                object[] atts = method.GetCustomAttributes(typeof(TransactionAttribute), true);
                if (atts.Length != 0)
                {
                    metaInfo.Add(method, atts[0] as TransactionAttribute);
                }
            }

            PopulateMetaInfoFromType(metaInfo, implementation.BaseType);
        }
        private void Register(Type implementation, TransactionMetaInfo metaInfo)
        {
            _type2MetaInfo[implementation] = metaInfo;
        }
    }
}
