using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace API_UsePrevention.Extensions
{
    public static class DatabaseServiceExtension
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<UserServiceContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                       .EnableSensitiveDataLogging()
            );
            return services;
        }
    }
}
