using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.Forum;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class ForumEventSubscriber
    {
        private IEntityManager _entityManager;

        public ForumEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(ForumCreated evnt)
        {
            var forum = new ForumEntity
            {
                Id = evnt.Id,
                Name = evnt.Name,
                TotalThread = evnt.State.TotalThread,
                TotalPost = evnt.State.TotalPost,
                LatestThreadId = evnt.State.LatestThreadId,
                LatestPostAuthorId = evnt.State.LatestPostAuthorId
            };
            _entityManager.Create(forum);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(ForumStateChanged evnt)
        {
            var forum = _entityManager.GetById<ForumEntity>(evnt.Id);
            forum.TotalThread = evnt.State.TotalThread;
            forum.TotalPost = evnt.State.TotalPost;
            forum.LatestThreadId = evnt.State.LatestThreadId;
            forum.LatestPostAuthorId = evnt.State.LatestPostAuthorId;
            _entityManager.Update(forum);
        }
    }
}
