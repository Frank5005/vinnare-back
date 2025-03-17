using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Configuration;

namespace Api.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
        {
            services.AddDbContextPool<VinnareDbContext>((serviceProvider, options) =>
            {
                var databaseSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                options.UseNpgsql(databaseSettings.DefaultConnection);
            });

            return services;
        }
    }
}
