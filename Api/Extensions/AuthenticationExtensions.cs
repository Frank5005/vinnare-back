using Microsoft.Extensions.DependencyInjection;

namespace Api.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services)
        {
            // Placeholder for JWT Setup (Will be implemented later)
            // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //     .AddJwtBearer(options =>
            //     {
            //         options.TokenValidationParameters = new TokenValidationParameters
            //         {
            //             // Configure JWT parameters here
            //         };
            //     });

            return services;
        }
    }
}
