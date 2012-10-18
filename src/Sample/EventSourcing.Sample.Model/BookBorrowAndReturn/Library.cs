using System;
using System.Collections.Generic;
using System.Linq;
using CodeSharp.EventSourcing;

namespace EventSourcing.Sample.Model.BookBorrowAndReturn
{
    public class Library : AggregateRoot<Guid>
    {
        private IList<BookStoreItem> _bookStoreItems = new List<BookStoreItem>();

        public Library() { }
        public Library(string name) : base(Guid.NewGuid())
        {
            Assert.IsNotNullOrWhiteSpace(name);
            OnEvent(new LibraryCreated(Id, name));
        }

        public string Name { get; private set; }
        public IEnumerable<BookStoreItem> BookStoreItems { get { return _bookStoreItems; } }

        /// <summary>
        /// 图书入库
        /// </summary>
        /// <param name="book"></param>
        /// <param name="count"></param>
        public void StoreBook(Book book, int count)
        {
            Assert.IsValid(book);
            Assert.Greater(count, 0);
            var bookStoreItem = _bookStoreItems.SingleOrDefault(x => x.BookId == book.Id);
            if (bookStoreItem == null)
            {
                OnEvent(new NewBookStored(Id, book.Id, count));
            }
            else
            {
                OnEvent(new BookCountUpdated(Id, book.Id, bookStoreItem.Count + count));
            }
        }
        /// <summary>
        /// 图书馆借出书
        /// </summary>
        /// <param name="book"></param>
        /// <param name="account"></param>
        /// <param name="count"></param>
        public void LendBook(Book book, LibraryAccount account, int count)
        {
            Assert.IsValid(book);
            Assert.Greater(count, 0);

            var bookStoreItem = _bookStoreItems.SingleOrDefault(x => x.BookId == book.Id);
            Assert.IsNotNull(bookStoreItem);
            Assert.GreaterOrEqual(bookStoreItem.Count, count);

            OnEvent(new BookLent(Id, book.Id, account.Id, count));
            OnAggregateRootCreated(new HandlingEvent(book, account, this, HandlingType.Borrow));
        }
        /// <summary>
        /// 图书馆接收归还的书
        /// </summary>
        /// <param name="book"></param>
        /// <param name="account"></param>
        /// <param name="count"></param>
        public void ReceiveBook(Book book, LibraryAccount account, int count)
        {
            Assert.IsValid(book);
            Assert.Greater(count, 0);

            var bookStoreItem = _bookStoreItems.SingleOrDefault(x => x.BookId == book.Id);
            Assert.IsNotNull(bookStoreItem);

            OnEvent(new BookReceived(Id, book.Id, account.Id, count));
            OnAggregateRootCreated(new HandlingEvent(book, account, this, HandlingType.Return));
        }
        /// <summary>
        /// 获取图书馆中某本书的库存信息
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns></returns>
        public BookStoreItem GetBookStoreItem(Guid bookId)
        {
            return _bookStoreItems.SingleOrDefault(x => x.BookId == bookId);
        }

        private void OnNewBookStored(NewBookStored evnt)
        {
            _bookStoreItems.Add(new BookStoreItem(evnt.BookId, Id, evnt.Count));
        }
        private void OnBookCountUpdated(BookCountUpdated evnt)
        {
            _bookStoreItems.Single(x => x.BookId == evnt.BookId).SetCount(evnt.Count);
        }
        private void OnBookLent(BookLent evnt)
        {
            var bookStoreItem = _bookStoreItems.Single(x => x.BookId == evnt.BookId);
            bookStoreItem.SetCount(bookStoreItem.Count - evnt.Count);
        }
        private void OnBookReceived(BookReceived evnt)
        {
            var bookStoreItem = _bookStoreItems.Single(x => x.BookId == evnt.BookId);
            bookStoreItem.SetCount(bookStoreItem.Count + evnt.Count);
        }
    }
    public class BookStoreItem
    {
        public Guid BookId { get; private set; }
        public Guid LibraryId { get; private set; }
        public int Count { get; private set; }

        protected BookStoreItem() { }
        public BookStoreItem(Guid bookId, Guid libraryId, int count)
        {
            BookId = bookId;
            LibraryId = libraryId;
            Count = count;
        }

        internal void SetCount(int count)
        {
            Assert.GreaterOrEqual(count, 0);
            Count = count;
        }

        public override bool Equals(object obj)
        {
            BookStoreItem item = obj as BookStoreItem;
            if (item == null)
            {
                return false;
            }
            if (item.LibraryId == LibraryId && item.BookId == BookId)
            {
                return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return LibraryId.GetHashCode() + BookId.GetHashCode();
        }
    }
    [Event]
    public class LibraryCreated
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public LibraryCreated(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
    [Event]
    public class NewBookStored
    {
        public Guid LibraryId { get; private set; }
        public Guid BookId { get; private set; }
        public int Count { get; private set; }

        public NewBookStored(Guid libraryId, Guid bookId, int count)
        {
            LibraryId = libraryId;
            BookId = bookId;
            Count = count;
        }
    }
    [Event]
    public class BookCountUpdated
    {
        public Guid LibraryId { get; private set; }
        public Guid BookId { get; private set; }
        public int Count { get; private set; }

        public BookCountUpdated(Guid libraryId, Guid bookId, int count)
        {
            LibraryId = libraryId;
            BookId = bookId;
            Count = count;
        }
    }
    [Event]
    public class BookLent
    {
        public Guid LibraryId { get; private set; }
        public Guid BookId { get; private set; }
        public Guid AccountId { get; private set; }
        public int Count { get; private set; }

        public BookLent(Guid libraryId, Guid bookId, Guid accountId, int count)
        {
            LibraryId = libraryId;
            BookId = bookId;
            AccountId = accountId;
            Count = count;
        }
    }
    [Event]
    public class BookReceived
    {
        public Guid LibraryId { get; private set; }
        public Guid BookId { get; private set; }
        public Guid AccountId { get; private set; }
        public int Count { get; private set; }

        public BookReceived(Guid libraryId, Guid bookId, Guid accountId, int count)
        {
            LibraryId = libraryId;
            BookId = bookId;
            AccountId = accountId;
            Count = count;
        }
    }
}
