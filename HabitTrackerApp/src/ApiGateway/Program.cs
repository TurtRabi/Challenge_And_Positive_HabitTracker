using Ocelot.Provider.Consul;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "default_secret_key"))
        };
    });

builder.Services.AddOcelot().AddConsul();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{

    c.SwaggerEndpoint("http://localhost:5001/swagger/v1/swagger.json", "User Service");
    c.SwaggerEndpoint("http://localhost:5002/swagger/v1/swagger.json", "Team Service");
    c.SwaggerEndpoint("http://localhost:5003/swagger/v1/swagger.json", "Task Service");
    c.SwaggerEndpoint("http://localhost:5004/swagger/v1/swagger.json", "Sticker Service");
    c.SwaggerEndpoint("http://localhost:5005/swagger/v1/swagger.json", "Share Service");
    c.SwaggerEndpoint("http://localhost:5006/swagger/v1/swagger.json", "Proof Service");
    c.SwaggerEndpoint("http://localhost:5007/swagger/v1/swagger.json", "Notification Service");
    c.SwaggerEndpoint("http://localhost:5008/swagger/v1/swagger.json", "Leaderboard Service");
    c.SwaggerEndpoint("http://localhost:5009/swagger/v1/swagger.json", "Friend Service");
    c.SwaggerEndpoint("http://localhost:5010/swagger/v1/swagger.json", "Chat Service");
    c.SwaggerEndpoint("http://localhost:5011/swagger/v1/swagger.json", "Assignment Service");


});
app.UseAuthentication();
app.UseAuthorization();

app.UseOcelot().Wait();

app.Run();