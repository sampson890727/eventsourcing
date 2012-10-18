using System;
using CodeSharp.EventSourcing;
using EventSourcing.Sample.Model.Forum;

namespace EventSourcing.Sample.Application
{
    public interface IPostService
    {
        Post Create(string body, Guid threadId, Guid authorId);
    }
    [Transactional]
    public class PostService : IPostService
    {
        private IRepository _repository;

        public PostService(IRepository repository)
        {
            _repository = repository;
        }

        [Transaction]
        Post IPostService.Create(string body, Guid threadId, Guid authorId)
        {
            var thread = _repository.GetById<Thread>(threadId);
            var author = _repository.GetById<User>(authorId);
            var post = new Post(body, thread, author);
            _repository.Add(post);
            return post;
        }
    }
}
