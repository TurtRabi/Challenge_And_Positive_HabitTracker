using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Repositories.GenericRepo
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly UserServiceContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(UserServiceContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task AddAnsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public  Task DeleteAnsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<T>> FindAnsyc(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAnsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public IQueryable<T> Query()
        {
            return  _dbSet.AsQueryable();
        }

        public Task UpdateAnsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
           
        }
    }
}
