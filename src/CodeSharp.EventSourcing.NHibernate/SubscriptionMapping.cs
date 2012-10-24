//Copyright (c) CodeSharp.  All rights reserved.

using NHibernate.Mapping.ByCode.Conformist;

namespace CodeSharp.EventSourcing.NHibernate
{
    public class SubscriptionMapping : ClassMapping<Subscription>
    {
        public SubscriptionMapping(string tableName)
        {
            Table(tableName);
            ComposedId(
                x =>
                {
                    x.Property(y => y.SubscriberEndpoint);
                    x.Property(y => y.MessageType);
                });
        }
    }
}
