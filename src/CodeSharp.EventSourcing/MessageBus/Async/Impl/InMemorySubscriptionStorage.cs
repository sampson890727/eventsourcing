using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// In memory implementation of the subscription storage
    /// </summary>
    [Component(LifeStyle=LifeStyle.Singleton)]
    public class InMemorySubscriptionStorage : ISubscriptionStorage
    {
        private readonly ConcurrentDictionary<Type, List<Address>> _storage = new ConcurrentDictionary<Type, List<Address>>();

        public void Init()
        {
        }

        void ISubscriptionStorage.Subscribe(Address client, Type messageType)
        {
            if (!_storage.ContainsKey(messageType))
            {
                _storage[messageType] = new List<Address>();
            }
            if (!_storage[messageType].Contains(client))
            {
                _storage[messageType].Add(client);
            }
        }
        void ISubscriptionStorage.Unsubscribe(Address client, Type messageType)
        {
            if (_storage.ContainsKey(messageType))
            {
                _storage[messageType].Remove(client);
            }
        }
        IEnumerable<Address> ISubscriptionStorage.GetSubscriberAddressesForMessage(Type messageType)
        {
            if (_storage.ContainsKey(messageType))
            {
                return _storage[messageType];
            }
            return new List<Address>();
        }
    }
}
