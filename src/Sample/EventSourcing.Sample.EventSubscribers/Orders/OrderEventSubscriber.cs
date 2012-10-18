using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.Orders;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class OrderEventSubscriber
    {
        private IEntityManager _entityManager;

        public OrderEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(OrderCreated evnt)
        {
            _entityManager.BuildAndSave<OrderEntity>(evnt);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(OrderItemAdded evnt)
        {
            _entityManager.BuildAndSave<OrderItemEntity>(evnt);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(OrderItemAmountUpdated evnt)
        {
            var orderItem = _entityManager.GetSingle<OrderItemEntity>(new { OrderId = evnt.OrderId, ProductId = evnt.ProductId });
            _entityManager.UpdateAndSave<OrderItemAmountUpdated>(orderItem, evnt, x => x.Amount);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(OrderItemRemoved evnt)
        {
            _entityManager.DeleteByQuery<OrderItemEntity>(new { OrderId = evnt.OrderId, ProductId = evnt.ProductId });
        }
    }
}
