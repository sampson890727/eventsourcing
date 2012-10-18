using System;
using CodeSharp.EventSourcing;
using EventSourcing.Sample.Model.Forum;
using ForumModel = EventSourcing.Sample.Model.Forum.Forum;

namespace EventSourcing.Sample.Application
{
    public interface IThreadService
    {
        Thread Create(string subject, string body, Guid forumId, Guid authorId, int marks);
        void ChangeContent(Guid id, string subject, string body, int marks);
        void MarkAsRecommended(Guid id);
        void UnMarkAsRecommended(Guid id);
        void Close(Guid id);
        void MarkAsDeleted(Guid id);
        void Stick(Guid id);
        void CancelStick(Guid id);
    }
    [Transactional]
    public class ThreadService : IThreadService
    {
        private IRepository _repository;

        public ThreadService(IRepository repository)
        {
            _repository = repository;
        }

        [Transaction]
        Thread IThreadService.Create(string subject, string body, Guid forumId, Guid authorId, int marks)
        {
            var forum = _repository.GetById<ForumModel>(forumId);
            var author = _repository.GetById<User>(authorId);
            var thread = new Thread(subject, body, forum, author, marks);
            _repository.Add(thread);
            return thread;
        }
        [Transaction]
        void IThreadService.ChangeContent(Guid id, string subject, string body, int marks)
        {
            _repository.GetById<Thread>(id).ChangeContent(subject, body, marks);
        }
        [Transaction]
        void IThreadService.MarkAsRecommended(Guid id)
        {
            _repository.GetById<Thread>(id).MarkAsRecommended();
        }
        [Transaction]
        void IThreadService.UnMarkAsRecommended(Guid id)
        {
            _repository.GetById<Thread>(id).MarkAsRecommended();
        }
        [Transaction]
        void IThreadService.Close(Guid id)
        {
            _repository.GetById<Thread>(id).Close();
        }
        [Transaction]
        void IThreadService.MarkAsDeleted(Guid id)
        {
            _repository.GetById<Thread>(id).MarkAsDeleted();
        }
        [Transaction]
        void IThreadService.Stick(Guid id)
        {
            _repository.GetById<Thread>(id).Stick();
        }
        [Transaction]
        void IThreadService.CancelStick(Guid id)
        {
            _repository.GetById<Thread>(id).CancelStick();
        }
    }
}
