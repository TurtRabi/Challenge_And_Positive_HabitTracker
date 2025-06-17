

using UserService.Repositories.GenericRepo;
using UserService.Repositories.UOW;

namespace API_UsePrevention.Extensions
{
    public static class RepositoryServiceExtension
    {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
