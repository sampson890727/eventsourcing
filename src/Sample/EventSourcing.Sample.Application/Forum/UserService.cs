using CodeSharp.EventSourcing;
using EventSourcing.Sample.Model.Forum;

namespace EventSourcing.Sample.Application
{
    public interface IUserService
    {
        User Create(string name);
    }
    [Transactional]
    public class UserService : IUserService
    {
        private IRepository _repository;

        public UserService(IRepository repository)
        {
            _repository = repository;
        }

        [Transaction]
        User IUserService.Create(string name)
        {
            var user = new User(name);
            _repository.Add(user);
            return user;
        }
    }
}
