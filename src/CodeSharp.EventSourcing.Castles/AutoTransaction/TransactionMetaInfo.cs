//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeSharp.EventSourcing.Castles
{
    public class TransactionMetaInfo : MarshalByRefObject
    {
        private readonly Dictionary<MethodInfo, TransactionAttribute> _method2Att;
        private readonly Dictionary<MethodInfo, String> _notTransactionalCache;
        private readonly object _lockerObject = new object();

        public TransactionMetaInfo()
        {
            _method2Att = new Dictionary<MethodInfo, TransactionAttribute>();
            _notTransactionalCache = new Dictionary<MethodInfo, String>();
        }

        #region MarshalByRefObject overrides

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"/> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"/> property.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. 
        ///                 </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure"/></PermissionSet>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        ///<summary>
        /// Adds a method info and the corresponding transaction attribute.
        ///</summary>
        public void Add(MethodInfo method, TransactionAttribute attribute)
        {
            _method2Att[method] = attribute;
        }
        ///<summary>
        /// Methods which needs transactions.
        ///</summary>
        public IEnumerable<MethodInfo> Methods
        {
            get
            {
                // quicker than array: http://blogs.msdn.com/ricom/archive/2006/03/12/549987.aspx
                var methods = new List<MethodInfo>(_method2Att.Count);
                methods.AddRange(_method2Att.Keys);
                return methods;
            }
        }
        /// <summary>
        /// True if methods is transactional. Otherwise else
        /// </summary>
        public bool Contains(MethodInfo info)
        {
            lock (_lockerObject)
            {
                if (_method2Att.ContainsKey(info)) return true;
                if (_notTransactionalCache.ContainsKey(info)) return false;

                if (info.DeclaringType.IsGenericType || info.IsGenericMethod)
                {
                    return IsGenericMethodTransactional(info);
                }

                return false;
            }
        }
        /// <summary>
        /// Returns the transaction metadata for a given method.
        /// </summary>
        public TransactionAttribute GetTransactionAttributeFor(MethodInfo methodInfo)
        {
            return _method2Att[methodInfo];
        }

        private bool IsGenericMethodTransactional(MethodInfo info)
        {
            object[] atts = info.GetCustomAttributes(typeof(TransactionAttribute), true);

            if (atts.Length != 0)
            {
                Add(info, atts[0] as TransactionAttribute);
                return true;
            }
            else
            {
                _notTransactionalCache[info] = string.Empty;
            }

            return false;
        }
    }
}
