using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.BookBorrowAndReturn;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class LibraryEventSubscriber
    {
        private IEntityManager _entityManager;

        public LibraryEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(LibraryCreated evnt)
        {
            _entityManager.BuildAndSave<LibraryEntity>(evnt);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(NewBookStored evnt)
        {
            _entityManager.BuildAndSave<BookStoreItemEntity>(evnt);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(BookCountUpdated evnt)
        {
            var bookStoreItem = _entityManager.GetSingle<BookStoreItemEntity>(new { LibraryId = evnt.LibraryId, BookId = evnt.BookId });
            _entityManager.UpdateAndSave<BookStoreItemEntity>(bookStoreItem, evnt, x => x.Count);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(BookLent evnt)
        {
            var bookStoreItem = _entityManager.GetSingle<BookStoreItemEntity>(new { LibraryId = evnt.LibraryId, BookId = evnt.BookId });
            bookStoreItem.Count -= evnt.Count;
            _entityManager.Update(bookStoreItem);
        }
        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(BookReceived evnt)
        {
            var bookStoreItem = _entityManager.GetSingle<BookStoreItemEntity>(new { LibraryId = evnt.LibraryId, BookId = evnt.BookId });
            bookStoreItem.Count += evnt.Count;
            _entityManager.Update(bookStoreItem);
        }
    }
}
