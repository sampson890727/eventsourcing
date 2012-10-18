using System;
using CodeSharp.EventSourcing;
using EventSourcing.Sample.Model.BookBorrowAndReturn;

namespace EventSourcing.Sample.Application
{
    public interface IBookService
    {
        /// <summary>
        /// 创建书本
        /// </summary>
        /// <param name="bookInfo"></param>
        /// <returns></returns>
        Book CreateBook(BookInfo bookInfo);
        /// <summary>
        /// 图书入库
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="count"></param>
        /// <param name="libraryId"></param>
        void AddBookToLibrary(Guid bookId, int count, Guid libraryId);
        /// <summary>
        /// 借书
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="accountId"></param>
        /// <param name="libraryId"></param>
        /// <param name="count"></param>
        void BorrowBook(Guid bookId, Guid accountId, Guid libraryId, int count);
        /// <summary>
        /// 还书
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="accountId"></param>
        /// <param name="libraryId"></param>
        /// <param name="count"></param>
        void ReturnBook(Guid bookId, Guid accountId, Guid libraryId, int count);
    }
    [Transactional]
    public class BookService : IBookService
    {
        private IRepository _repository;

        public BookService(IRepository repository)
        {
            _repository = repository;
        }

        [Transaction]
        Book IBookService.CreateBook(BookInfo bookInfo)
        {
            var book = new Book(bookInfo);
            _repository.Add(book);
            return book;
        }
        [Transaction]
        void IBookService.AddBookToLibrary(Guid bookId, int count, Guid libraryId)
        {
            var book = _repository.GetById<Book>(bookId);
            var libarary = _repository.GetByIdWithLock<Library>(libraryId);
            libarary.StoreBook(book, count);
        }
        [Transaction]
        void IBookService.BorrowBook(Guid bookId, Guid accountId, Guid libraryId, int count)
        {
            var book = _repository.GetById<Book>(bookId);
            var library = _repository.GetByIdWithLock<Library>(libraryId);
            var account = _repository.GetByIdWithLock<LibraryAccount>(accountId);
            new BorrowBookContext(account.ActAs<IBorrower>(), library).Interaction(book, count);
        }
        [Transaction]
        void IBookService.ReturnBook(Guid bookId, Guid accountId, Guid libraryId, int count)
        {
            var book = _repository.GetById<Book>(bookId);
            var library = _repository.GetByIdWithLock<Library>(libraryId);
            var account = _repository.GetByIdWithLock<LibraryAccount>(accountId);
            new ReturnBookContext(account.ActAs<IBorrower>(), library).Interaction(book, count);
        }
    }

    //data,role,context,interaction

    public class BorrowBookContext
    {
        private IBorrower _borrower;
        private Library _library;

        public BorrowBookContext(IBorrower borrower, Library library)
        {
            _borrower = borrower;
            _library = library;
        }

        public void Interaction(Book book, int count)
        {
            _borrower.BorrowBook(_library, book, count);
        }
    }
    public class ReturnBookContext
    {
        private IBorrower _borrower;
        private Library _library;

        public ReturnBookContext(IBorrower borrower, Library library)
        {
            _borrower = borrower;
            _library = library;
        }

        public void Interaction(Book book, int count)
        {
            _borrower.ReturnBook(_library, book, count);
        }
    }
}
