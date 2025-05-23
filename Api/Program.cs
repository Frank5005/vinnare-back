using Api.Extensions;
using Api.Middleware;
using Microsoft.IdentityModel.Tokens;
using Shared.Configuration;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("Security"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<OpenTelemetrySettings>(builder.Configuration.GetSection("OpenTelemetry"));
builder.Services.AddSwaggerConfiguration();
builder.Services.AddDatabaseConfiguration();
builder.Services.AddApplicationServices();
builder.Services.AddLoggingConfiguration(builder.Configuration);
builder.AddOpenTemlemetryConfiguration();
builder.Services.AddRateLimiterConfiguration();
builder.Services.AddAuthenticationConfiguration();
builder.Services.AddAuthorization();


// CORS policy for the frontend application
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:8080",
            "https://main.d3hcv6qzhmyahb.amplifyapp.com",
            "https://5586-3-147-45-32.ngrok-free.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed(origin => true); // Esto es temporal para debug
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://yourdomain.com"; // Cambia esto por tu autoridad
        options.Audience = "https://yourdomain.com"; // Cambia esto por tu audiencia
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        };
    });


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

//app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Urls.Add("http://*:8080"); // .NET escuche en el puerto 8080 (por ejemplo)

app.Run();
