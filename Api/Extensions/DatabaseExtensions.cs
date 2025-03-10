using Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<VinnareDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
    }
}
