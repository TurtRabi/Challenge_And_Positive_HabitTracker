using UserService.Models;
using UserService.Repositories.GenericRepo;

namespace UserService.Repositories.UOW
{
    public interface IUnitOfWork
    {
        IGenericRepository<User> user { get; }
        IGenericRepository<Role> role { get; }
        IGenericRepository<UserProvider> user_provider { get; }
        Task<int> CommitAsync();
    }
}
