using CodeSharp.EventSourcing;
using EventSourcing.Sample.Model.Orders;

namespace EventSourcing.Sample.Application
{
    public interface IProductService
    {
        Product Create(string name, string description, double price);
    }
    [Transactional]
    public class ProductService : IProductService
    {
        private IRepository _repository;

        public ProductService(IRepository repository)
        {
            _repository = repository;
        }

        [Transaction]
        public Product Create(string name, string description, double price)
        {
            var product = new Product(name, description, price);
            _repository.Add(product);
            return product;
        }
    }
}
