using FluentNHibernate.Mapping;
using NHibernate.Mapping.ByCode.Conformist;

namespace CodeSharp.EventSourcing.NHibernate
{
    public abstract class AggregateRootEventMap<TAggregateRoot> : ClassMap<AggregateRootEvent<TAggregateRoot>> where TAggregateRoot : AggregateRoot
    {
        public AggregateRootEventMap(string tableName)
        {
            Table(tableName);
            CompositeId()
                .KeyProperty(x => x.AggregateRootName)
                .KeyProperty(x => x.AggregateRootId)
                .KeyProperty(x => x.Version);
            Map(x => x.Name);
            Map(x => x.Data);
            Map(x => x.OccurredTime);
        }
    }

    public class AggregateRootEventMapping<TAggregateRoot> : ClassMapping<AggregateRootEvent<TAggregateRoot>> where TAggregateRoot : AggregateRoot
    {
        public AggregateRootEventMapping(string tableName)
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
            Property(x => x.Data);
            Property(x => x.OccurredTime);
        }
    }
}
