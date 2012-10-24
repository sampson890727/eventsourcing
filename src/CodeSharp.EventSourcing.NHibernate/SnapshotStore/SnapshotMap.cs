//Copyright (c) CodeSharp.  All rights reserved.

using FluentNHibernate.Mapping;
using NHibernate.Mapping.ByCode.Conformist;

namespace CodeSharp.EventSourcing.NHibernate
{
    public abstract class AggregateRootSnapshotMap : ClassMap<Snapshot>
    {
        public AggregateRootSnapshotMap(string tableName)
        {
            Table(tableName);
            CompositeId()
                .KeyProperty(x => x.AggregateRootName)
                .KeyProperty(x => x.AggregateRootId)
                .KeyProperty(x => x.Version);
            Map(x => x.Name);
            Map(x => x.SerializedData);
            Map(x => x.CreatedTime);
        }
    }

    public class AggregateRootSnapshotMapping : ClassMapping<Snapshot>
    {
        public AggregateRootSnapshotMapping(string tableName)
        {
            Table(tableName);
            ComposedId(
                x =>
                {
                    x.Property(y => y.AggregateRootName);
                    x.Property(y => y.AggregateRootId);
                    x.Property(y => y.Version);
                });
            Property(x => x.Name);
            Property(x => x.SerializedData);
            Property(x => x.CreatedTime);
        }
    }
}
