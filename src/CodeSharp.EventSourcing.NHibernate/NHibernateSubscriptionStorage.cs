//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CodeSharp.EventSourcing.NHibernate;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// NHibernate implementation of the subscription storage
    /// </summary>
    public class NHibernateSubscriptionStorage : ISubscriptionStorage
    {
        private INHibernateSessionManager _sessionManager;

        public NHibernateSubscriptionStorage(INHibernateSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public void Init()
        {
        }

        void ISubscriptionStorage.Subscribe(Address client, Type messageType)
        {
            //if (!_storage.ContainsKey(messageType))
            //{
            //    _storage[messageType] = new List<Address>();
            //}
            //if (!_storage[messageType].Contains(client))
            //{
            //    _storage[messageType].Add(client);
            //}
        }
        void ISubscriptionStorage.Unsubscribe(Address client, Type messageType)
        {
            //if (_storage.ContainsKey(messageType))
            //{
            //    _storage[messageType].Remove(client);
            //}
        }
        IEnumerable<Address> ISubscriptionStorage.GetSubscriberAddressesForMessage(Type messageType)
        {
            //if (_storage.ContainsKey(messageType))
            //{
            //    return _storage[messageType];
            //}
            return new List<Address>();
        }
    }

    /// <summary>
    /// Entity containing subscription data
    /// </summary>
    public class Subscription
    {
        public virtual string SubscriberEndpoint { get; set; }
        public virtual string MessageType { get; set; }

        public override bool Equals(object obj)
        {
            var subscription = obj as Subscription;
            if (subscription == null)
            {
                return false;
            }
            return subscription.SubscriberEndpoint == SubscriberEndpoint && subscription.MessageType == MessageType;
        }
        public override int GetHashCode()
        {
            return SubscriberEndpoint.GetHashCode() + MessageType.GetHashCode();
        }
    }
}
