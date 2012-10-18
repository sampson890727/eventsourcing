using System;
using System.Collections.Generic;
using System.Linq;
using CodeSharp.EventSourcing;

namespace EventSourcing.Sample.Model.Orders
{
    public class Order : AggregateRoot<Guid>
    {
        private IList<OrderItem> _items = new List<OrderItem>();

        public string Customer { get; private set; }
        public IEnumerable<OrderItem> Items { get { return _items; } }
        public DateTime CreatedTime { get; private set; }
        public double TotalPrice
        {
            get
            {
                double totalPrice = 0;
                _items.ForEach(item => totalPrice += item.GetTotalPrice());
                return totalPrice;
            }
        }

        public Order() { }
        public Order(string customer) : base(Guid.NewGuid())
        {
            Assert.IsNotNullOrWhiteSpace(customer);
            OnEvent(new OrderCreated(Id, customer, DateTime.Now));
        }

        public OrderItem AddItem(Product product, int amount)
        {
            Assert.IsValid(product);
            Assert.Greater(amount, 0);

            var item = _items.SingleOrDefault(x => x.ProductId == product.Id);

            if (item == null)
            {
                OnEvent(new OrderItemAdded(Id, product.Id, product.Price, amount));
            }
            else
            {
                OnEvent(new OrderItemAmountUpdated(Id, item.ProductId, item.Amount + amount));
            }

            return _items.SingleOrDefault(x => x.ProductId == product.Id);
        }
        public void UpdateItemAmount(Guid productId, int amount)
        {
            var item = _items.SingleOrDefault(x => x.ProductId == productId);
            Assert.IsNotNull(item);
            Assert.Greater(amount, 0);
            OnEvent(new OrderItemAmountUpdated(Id, item.ProductId, amount));
        }
        public void RemoveItem(Guid productId)
        {
            var item = _items.SingleOrDefault(x => x.ProductId == productId);
            Assert.IsNotNull(item);
            OnEvent(new OrderItemRemoved(Id, item.ProductId));
        }
        public OrderItem GetItem(Guid productId)
        {
            return _items.SingleOrDefault(x => x.ProductId == productId);
        }

        private void OnOrderItemAdded(OrderItemAdded evnt)
        {
            _items.Add(new OrderItem(evnt.OrderId, evnt.ProductId, evnt.Price, evnt.Amount));
        }
        private void OnOrderItemAmountUpdated(OrderItemAmountUpdated evnt)
        {
            _items.Single(x => x.ProductId == evnt.ProductId).SetAmount(evnt.Amount);
        }
        private void OnOrderItemRemoved(OrderItemRemoved evnt)
        {
            _items.Remove(_items.Single(x => x.ProductId == evnt.ProductId));
        }
    }
    public class OrderItem
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public double Price { get; private set; }
        public int Amount { get; private set; }

        protected OrderItem() { }
        public OrderItem(Guid orderId, Guid productId, double price, int amount)
        {
            Assert.Greater(price, 0.0D);
            Assert.Greater(amount, 0.0D);

            OrderId = orderId;
            ProductId = productId;
            Price = price;
            Amount = amount;
        }

        internal void SetAmount(int amount)
        {
            Assert.Greater(amount, 0.0D);
            Amount = amount;
        }
        public double GetTotalPrice()
        {
            return Price * Amount;
        }
    }
    [Event]
    public class OrderCreated
    {
        public OrderCreated(Guid id, string customer, DateTime createdTime)
        {
            this.Id = id;
            this.Customer = customer;
            this.CreatedTime = createdTime;
        }

        public Guid Id { get; private set; }
        public string Customer { get; private set; }
        public DateTime CreatedTime { get; private set; }
    }
    [Event]
    public class OrderItemAdded
    {
        public OrderItemAdded(Guid orderId, Guid productId, double price, int amount)
        {
            this.OrderId = orderId;
            this.ProductId = productId;
            this.Price = price;
            this.Amount = amount;
        }

        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public double Price { get; private set; }
        public int Amount { get; private set; }
    }
    [Event]
    public class OrderItemAmountUpdated
    {
        public OrderItemAmountUpdated(Guid orderId, Guid productId, int amount)
        {
            this.OrderId = orderId;
            this.ProductId = productId;
            this.Amount = amount;
        }

        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Amount { get; private set; }
    }
    [Event]
    public class OrderItemRemoved
    {
        public OrderItemRemoved(Guid orderId, Guid productId)
        {
            this.OrderId = orderId;
            this.ProductId = productId;
        }

        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
    }
}
