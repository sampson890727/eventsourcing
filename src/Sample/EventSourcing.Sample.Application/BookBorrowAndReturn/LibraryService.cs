using CodeSharp.EventSourcing;
using EventSourcing.Sample.Model.BookBorrowAndReturn;

namespace EventSourcing.Sample.Application
{
    public interface ILibraryService
    {
        Library Create(string name);
    }

    [Transactional]
    public class LibraryService : ILibraryService
    {
        private IRepository _repository;

        public LibraryService(IRepository repository)
        {
            _repository = repository;
        }

        [Transaction]
        Library ILibraryService.Create(string name)
        {
            var library = new Library(name);
            _repository.Add(library);
            return library;
        }
    }
}
