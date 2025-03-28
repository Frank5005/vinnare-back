using Api.Services;
using Services;
using Services.Builders;
using Services.Interfaces;
using Services.Utils;
namespace Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IWishListService, WishListService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IPurchaseService, PurchaseService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ICartPurchaseBuilderFactory, CartPurchaseBuilderFactory>();

            return services;
        }
    }
}
