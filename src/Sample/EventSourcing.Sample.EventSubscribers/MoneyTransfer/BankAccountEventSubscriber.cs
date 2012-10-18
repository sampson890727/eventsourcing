using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.MoneyTransfer;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class BankAccountEventSubscriber
    {
        private IEntityManager _entityManager;

        public BankAccountEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(BankAccountCreated evnt)
        {
            _entityManager.BuildAndSave<BankAccountEntity>(evnt);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(AccountBalanceUpdated evnt)
        {
            var bankAccount = _entityManager.GetById<BankAccountEntity>(evnt.BankAccountId);
            _entityManager.UpdateAndSave<AccountBalanceUpdated>(bankAccount, evnt, x => x.Balance);
        }
    }
}
