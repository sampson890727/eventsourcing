using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.Forum;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class PostEventSubscriber
    {
        private IEntityManager _entityManager;

        public PostEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(PostCreated evnt)
        {
            _entityManager.BuildAndSave<PostEntity>(evnt);
        }
    }
}
