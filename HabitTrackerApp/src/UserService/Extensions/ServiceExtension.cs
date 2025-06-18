
using StackExchange.Redis;
using UserService.Services.Redis;
using UserService.Services.ServiceRole;
using UserService.Services.ServiceUser;
using UserService.Services.UserProviderService;


namespace API_UsePrevention.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserServices>();
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379"));
            services.AddScoped<IRedisService, RedisService>();
            services.AddScoped<IUserProviderService, UserProviderService>();
            return services;
        }
    }
}
