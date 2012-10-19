//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 用于存储消息的订阅者地址的接口定义
    /// </summary>
    public interface ISubscriptionStorage
    {
        /// <summary>
        /// Notifies the subscription storage that now is the time to performany initialization work
        /// </summary>
        void Init();

        /// <summary>
        /// Subscribes the given client address to messages of the given type.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="messageTypes"></param>
        void Subscribe(Address client, Type messageType);

        /// <summary>
        /// Unsubscribes the given client address from messages of the given type.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="messageTypes"></param>
        void Unsubscribe(Address client, Type messageType);

        /// <summary>
        /// Returns a list of addresses of subscribers that previously requested to be notified
        /// of messages of the given message type.
        /// </summary>
        /// <param name="messageTypes"></param>
        /// <returns></returns>
        IEnumerable<Address> GetSubscriberAddressesForMessage(Type messageType);
    }
}
