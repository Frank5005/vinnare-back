using Services;
using Services.Interfaces;

namespace Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}
