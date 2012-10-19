//Copyright (c) CodeSharp.  All rights reserved.

using System.Collections.Generic;
using NHibernate;

namespace CodeSharp.EventSourcing.NHibernate
{
    /// <summary>
    /// 一个基于NHibernate实现的用于查询事件的接口
    /// </summary>
    public interface INHibernateEventQueryService
    {
        IList<T> QueryEvents<T>(ICriteria criteria) where T : AggregateRootEvent;
    }
    public class NHibernateEventQueryService : INHibernateEventQueryService
    {
        private IJsonSerializer _eventSerializer;
        private ITypeNameMapper _typeNameMapper;

        public NHibernateEventQueryService(IJsonSerializer eventSerializer, ITypeNameMapper typeNameMapper)
        {
            _eventSerializer = eventSerializer;
            _typeNameMapper = typeNameMapper;
        }

        public IList<T> QueryEvents<T>(ICriteria criteria) where T : AggregateRootEvent
        {
            var eventList = criteria.List<T>();

            foreach (var evnt in eventList)
            {
                evnt.AggregateRootType = _typeNameMapper.GetType(NameTypeMappingType.AggregateRootMapping, evnt.AggregateRootName);
                evnt.Event = _eventSerializer.Deserialize(evnt.Data, _typeNameMapper.GetType(NameTypeMappingType.EventMapping, evnt.Name));
            }

            return eventList;
        }
    }
}
