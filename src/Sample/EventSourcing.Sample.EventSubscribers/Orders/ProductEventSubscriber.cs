using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.Orders;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class ProductEventSubscriber
    {
        private IEntityManager _entityManager;

        public ProductEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(ProductCreated evnt)
        {
            _entityManager.BuildAndSave<ProductEntity>(evnt);
        }
    }
}
