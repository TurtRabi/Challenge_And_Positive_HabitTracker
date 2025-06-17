using API_UsePrevention.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


builder.Services.AddDatabaseServices(builder.Configuration);

builder.Services.AddRepositoryServices();

builder.Services.AddAppServices();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();



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

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.RegisterWithConsul("userservice", 80);

app.MapControllers();
app.Run();