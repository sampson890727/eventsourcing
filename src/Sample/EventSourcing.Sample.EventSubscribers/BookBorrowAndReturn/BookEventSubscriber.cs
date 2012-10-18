using CodeSharp.EventSourcing;
using EventSourcing.Sample.Entities;
using EventSourcing.Sample.Model.BookBorrowAndReturn;

namespace EventSourcing.Sample.EventSubscribers
{
    [Transactional]
    public class BookEventSubscriber
    {
        private IEntityManager _entityManager;

        public BookEventSubscriber(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        [Transaction]
        [AsyncHandler]
        protected virtual void Handle(BookCreated evnt)
        {
            var book = new BookEntity
            {
                Id = evnt.Id,
                Name = evnt.BookInfo.Name,
                Description = evnt.BookInfo.Description,
                Author = evnt.BookInfo.Author,
                ISBN = evnt.BookInfo.ISBN,
                Publisher = evnt.BookInfo.Publisher
            };
            _entityManager.Create(book);
        }
    }
}
