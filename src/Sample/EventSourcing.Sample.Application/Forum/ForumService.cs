using CodeSharp.EventSourcing;
using ForumModel = EventSourcing.Sample.Model.Forum.Forum;

namespace EventSourcing.Sample.Application
{
    public interface IForumService
    {
        ForumModel Create(string name);
    }
    [Transactional]
    public class ForumService : IForumService
    {
        private IRepository _repository;

        public ForumService(IRepository repository)
        {
            _repository = repository;
        }

        [Transaction]
        ForumModel IForumService.Create(string name)
        {
            var forum = new ForumModel(name);
            _repository.Add(forum);
            return forum;
        }
    }
}
