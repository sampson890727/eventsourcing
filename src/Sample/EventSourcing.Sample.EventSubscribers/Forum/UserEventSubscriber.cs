using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.Forum;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class UserEventSubscriber
    {
        private IEntityManager _entityManager;

        public UserEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(UserCreated evnt)
        {
            _entityManager.BuildAndSave<UserEntity>(evnt);
        }
    }
}
