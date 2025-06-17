using UserService.Models;
using UserService.Repositories.GenericRepo;

namespace UserService.Repositories.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UserServiceContext _context;
        public IGenericRepository<User> user { get; }

        public IGenericRepository<Role> role { get; }

        public IGenericRepository<UserProvider> user_provider { get; }
        public UnitOfWork(UserServiceContext context)
        {
            _context = context;
            user = new GenericRepository<User>(_context);
            role = new GenericRepository<Role>(_context);
            user_provider = new GenericRepository<UserProvider>(_context);
        }

        public Task<int> CommitAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
