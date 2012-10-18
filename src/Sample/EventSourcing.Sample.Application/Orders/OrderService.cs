using System;
using CodeSharp.EventSourcing;
using EventSourcing.Sample.Model.Orders;

namespace EventSourcing.Sample.Application
{
    public interface IOrderService
    {
        Order Create(string customer);
        OrderItem AddItem(Guid orderId, Guid productId, int amount);
        void UpdateOrderItemAmount(Guid orderId, Guid productId, int amount);
        void RemoveOrderItem(Guid orderId, Guid productId);
    }
    [Transactional]
    public class OrderService : IOrderService
    {
        private IRepository _repository;

        public OrderService(IRepository repository)
        {
            _repository = repository;
        }

        [Transaction]
        public Order Create(string customer)
        {
            var order = new Order(customer);
            _repository.Add(order);
            return order;
        }
        [Transaction]
        public OrderItem AddItem(Guid orderId, Guid productId, int amount)
        {
            var product = _repository.GetById<Product>(productId);
            return _repository.GetById<Order>(orderId).AddItem(product, amount);
        }
        [Transaction]
        public void RemoveOrderItem(Guid orderId, Guid productId)
        {
            _repository.GetById<Order>(orderId).RemoveItem(productId);
        }
        [Transaction]
        public void UpdateOrderItemAmount(Guid orderId, Guid productId, int amount)
        {
            _repository.GetById<Order>(orderId).UpdateItemAmount(productId, amount);
        }
    }
}
