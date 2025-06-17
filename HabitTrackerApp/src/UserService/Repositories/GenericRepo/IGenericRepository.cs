using System.Linq.Expressions;

namespace UserService.Repositories.GenericRepo
{
    public interface IGenericRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAnsync(int id);
        Task AddAnsync(T entity);
        Task DeleteAnsync(T entity);
        Task UpdateAnsync(T entity);

        IQueryable<T> Query();
        Task<IEnumerable<T>> FindAnsyc(Expression<Func<T, bool>> predicate);
        Task AddRangeAsync(IEnumerable<T> entities);
    }
}
