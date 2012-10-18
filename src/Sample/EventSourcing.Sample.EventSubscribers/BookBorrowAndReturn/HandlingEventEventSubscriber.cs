using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.BookBorrowAndReturn;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class HandlingEventEventSubscriber
    {
        private IEntityManager _entityManager;

        public HandlingEventEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(HandlingEventCreated evnt)
        {
            _entityManager.BuildAndSave<HandlingEventEntity>(evnt);
        }
    }
}
