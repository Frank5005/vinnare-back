using Api.Extensions;
using Api.Middleware;
using Shared.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Modular Configuration
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddLoggingConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfiguration();
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("Security"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
