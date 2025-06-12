using Consul;
using Microsoft.AspNetCore.Builder;

public static class ConsulExtensions
{
    public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, string serviceName, int port)
    {
        var consulClient = new ConsulClient(cfg =>
        {
            cfg.Address = new Uri("http://consul:8500");
        });

        var registration = new AgentServiceRegistration()
        {
            ID = $"{serviceName}-{Guid.NewGuid()}",
            Name = serviceName,
            Address = serviceName,
            Port = port,
            Tags = new[] { serviceName }
        };

        consulClient.Agent.ServiceRegister(registration).Wait();

        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Register(() =>
        {
            consulClient.Agent.ServiceDeregister(registration.ID).Wait();
        });

        return app;
    }
}
