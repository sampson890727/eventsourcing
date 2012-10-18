using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.BookBorrowAndReturn;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class LibraryAccountEventSubscriber
    {
        private IEntityManager _entityManager;

        public LibraryAccountEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(AccountCreated evnt)
        {
            _entityManager.BuildAndSave<LibraryAccountEntity>(evnt);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(BookBorrowed evnt)
        {
            var borrowedBook = _entityManager.GetSingleOrDefault<BorrowedBookEntity>(new { AccountId = evnt.AccountId, BookId = evnt.BookId });
            if (borrowedBook == null)
            {
                _entityManager.BuildAndSave<BorrowedBookEntity>(evnt);
            }
            else
            {
                borrowedBook.Count += evnt.Count;
                _entityManager.Update(borrowedBook);
            }
        }
    }
}
