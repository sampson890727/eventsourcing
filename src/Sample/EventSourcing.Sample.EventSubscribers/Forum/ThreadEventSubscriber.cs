using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.Forum;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class ThreadEventSubscriber
    {
        private IEntityManager _entityManager;

        public ThreadEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(ThreadCreated evnt)
        {
            var thread = _entityManager.Build<ThreadEntity>(evnt);
            thread.IsStick = evnt.StickInfo.IsStick;
            thread.StickDate = evnt.StickInfo.StickDate;
            _entityManager.Create(thread);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(ContentChanged evnt)
        {
            var thread = _entityManager.GetById<ThreadEntity>(evnt.Id);
            _entityManager.UpdateAndSave<ContentChanged>(thread, evnt,
                x => x.Subject,
                x => x.Body,
                x => x.Marks);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(ThreadStatusChanged evnt)
        {
            var thread = _entityManager.GetById<ThreadEntity>(evnt.Id);
            _entityManager.UpdateAndSave<ThreadStatusChanged>(thread, evnt, x => x.Status);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(ThreadStickInfoChanged evnt)
        {
            var thread = _entityManager.GetById<ThreadEntity>(evnt.Id);
            thread.IsStick = evnt.StickInfo.IsStick;
            thread.StickDate = evnt.StickInfo.StickDate;
            _entityManager.Update(thread);
        }
    }
}
