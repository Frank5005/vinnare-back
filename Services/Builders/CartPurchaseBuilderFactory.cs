using Data;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Services.Builders
{
    public class CartPurchaseBuilderFactory : ICartPurchaseBuilderFactory
    {
        private readonly VinnareDbContext _dbContext;
        private readonly ICouponService _couponService;
        private readonly ILogger<CartPurchaseBuilder> _logger;
        public CartPurchaseBuilderFactory(VinnareDbContext dbContext, ICouponService couponService, ILogger<CartPurchaseBuilder> logger)
        {
            _dbContext = dbContext;
            _couponService = couponService;
            _logger = logger;
        }

        public ICartPurchaseBuilder Create(Guid userId)
        {
            return new CartPurchaseBuilder(_dbContext, userId, _couponService, _logger);
        }
    }

}
