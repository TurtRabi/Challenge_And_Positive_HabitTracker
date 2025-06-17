
using UserService.Services.ServiceRole;
using UserService.Services.ServiceUser;


namespace API_UsePrevention.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserServices>();
            return services;
        }
    }
}
