using CodeSharp.EventSourcing;
using EventSourcing.Sample.Model.BookBorrowAndReturn;

namespace EventSourcing.Sample.Application
{
    public interface ILibraryAccountService
    {
        LibraryAccount Create(string number, string owner);
    }
    [Transactional]
    public class LibraryAccountService : ILibraryAccountService
    {
        private IRepository _repository;

        public LibraryAccountService(IRepository repository)
        {
            _repository = repository;
        }

        [Transaction]
        LibraryAccount ILibraryAccountService.Create(string number, string owner)
        {
            var account = new LibraryAccount(number, owner);
            _repository.Add(account);
            return account;
        }
    }
}
