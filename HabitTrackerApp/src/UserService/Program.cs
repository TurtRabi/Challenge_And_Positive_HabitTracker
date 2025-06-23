using API_UsePrevention.Extensions;
using UserService.Common;
using UserService.Dto.Email;
using UserService.Middlewares;
using UserService.Services.ServiceSendEmail;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


builder.Services.AddDatabaseServices(builder.Configuration);

builder.Services.AddRepositoryServices();

builder.Services.AddAppServices();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("Authentication:Google"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));




builder.Services.AddSwaggerWithJwt();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b =>
    {
        b.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseMiddleware<TokenRefreshMiddleware>();

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.RegisterWithConsul("userservice", 80);

app.MapControllers();
app.Run();