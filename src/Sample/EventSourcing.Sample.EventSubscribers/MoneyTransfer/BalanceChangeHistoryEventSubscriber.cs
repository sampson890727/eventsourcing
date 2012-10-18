using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.MoneyTransfer;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class BalanceChangeHistoryEventSubscriber
    {
        private IEntityManager _entityManager;

        public BalanceChangeHistoryEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(BalanceChangeHistoryCreated evnt)
        {
            _entityManager.BuildAndSave<BalanceChangeHistoryEntity>(evnt);
        }
    }
}
