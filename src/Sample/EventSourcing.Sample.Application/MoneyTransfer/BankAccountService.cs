using System;
using CodeSharp.EventSourcing;
using EventSourcing.Sample.Model.MoneyTransfer;

namespace EventSourcing.Sample.Application
{
    public interface IBankAccountService
    {
        BankAccount CreateBankAccount(string customer, string accountNumber);
        void DepositMoney(Guid bankAccountId, double amount);
        void WithdrawMoney(Guid bankAccountId, double amount);
        void TransferMoney(Guid sourceBankAccountId, Guid targetBankAccountId, double amount);
    }
    [Transactional]
    public class BankAccountService : IBankAccountService
    {
        private IRepository _repository;
        private ITransferMoneyService _transferMoneyService;

        public BankAccountService(IRepository repository, ITransferMoneyService transferMoneyService)
        {
            _repository = repository;
            _transferMoneyService = transferMoneyService;
        }

        [Transaction]
        public BankAccount CreateBankAccount(string customer, string accountNumber)
        {
            var bankAccount = new BankAccount(customer, accountNumber);
            _repository.Add(bankAccount);
            return bankAccount;
        }
        [Transaction]
        public void DepositMoney(Guid bankAccountId, double amount)
        {
            _repository.GetByIdWithLock<BankAccount>(bankAccountId).Deposit(amount);
        }
        [Transaction]
        public void WithdrawMoney(Guid bankAccountId, double amount)
        {
            _repository.GetByIdWithLock<BankAccount>(bankAccountId).Withdraw(amount);
        }
        [Transaction]
        public void TransferMoney(Guid sourceBankAccountId, Guid targetBankAccountId, double amount)
        {
            var sourceAccount = _repository.GetByIdWithLock<BankAccount>(sourceBankAccountId);
            var targetAccount = _repository.GetByIdWithLock<BankAccount>(targetBankAccountId);
            _transferMoneyService.TransferMoney(sourceAccount, targetAccount, amount);
        }
    }
}
