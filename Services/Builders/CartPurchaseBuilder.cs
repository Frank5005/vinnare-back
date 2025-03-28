using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;

namespace Services.Builders
{

    public class CartPurchaseBuilder : ICartPurchaseBuilder
    {
        private readonly VinnareDbContext _dbContext;
        private readonly Guid _userId;
        private List<Cart> _cartItems = new();
        private decimal _totalPricePreDiscount;
        private decimal _finalPrice;
        private decimal _totalAfterDiscount;
        private decimal _discount;
        private CouponData _couponData;
        private decimal _shipping_cost = 10m; //not implemented yet
        private IDbContextTransaction? _transaction;
        private readonly ICouponService _couponService;
        private PurchaseResponse? _finalPurchase;
        private readonly ILogger<CartPurchaseBuilder> _logger;
        public CartPurchaseBuilder(VinnareDbContext dbContext, Guid userId, ICouponService couponService, ILogger<CartPurchaseBuilder> logger)
        {
            _dbContext = dbContext;
            _userId = userId;
            _couponService = couponService;
            _logger = logger;
        }
        public async Task<ICartPurchaseBuilder> LoadCartAsync()
        {
            _cartItems = await _dbContext.Carts
                .Where(c => c.UserId == _userId)
                .Include(c => c.Product)
                .ToListAsync();


            if (_cartItems.Count == 0)
            {
                throw new NotFoundException("Cart is empty; cannot proceed.");
            }

            return this;
        }
        public ICartPurchaseBuilder ValidateApproved()
        {
            foreach (var item in _cartItems)
            {
                if (!item.Product.Approved)
                {
                    throw new GoneException($"Product {item.ProductId} is not available for purchase anymore.");
                }
            }
            return this;
        }
        public ICartPurchaseBuilder ValidateStock()
        {
            foreach (var item in _cartItems)
            {
                if (item.Quantity > item.Product.Available)
                {
                    throw new GoneException($"Product {item.ProductId} is out of stock or not enough stock.");
                }
            }
            return this;
        }

        public ICartPurchaseBuilder CalcBasePrice()
        {

            _totalPricePreDiscount = _cartItems
                .Sum(ci => ci.Product.Price * ci.Quantity);
            return this;
        }

        public async Task<ICartPurchaseBuilder> FindCoupon(string? coupon_code)
        {
            var couponDto = await _couponService.GetCouponByCode(coupon_code);
            if (couponDto != null)
            {
                _discount = couponDto.DiscountPercentage / 100m;
                _couponData = new CouponData
                {
                    coupon_code = couponDto.Code,
                    discount_percentage = couponDto.DiscountPercentage
                };
            }
            return this;
        }
        public ICartPurchaseBuilder CalcFinalPrice()
        {
            if (_discount != 0)
            {
                _totalAfterDiscount = _totalPricePreDiscount * _discount;
            }
            else
            {
                _totalAfterDiscount = _totalPricePreDiscount;
            }
            _finalPrice = _totalAfterDiscount + _shipping_cost;
            return this;
        }

        public ICartPurchaseBuilder DecrementStock()
        {
            foreach (var item in _cartItems)
            {
                item.Product.Available -= item.Quantity;
            }
            return this;
        }

        public ICartPurchaseBuilder CreatePurchase()
        {
            var purchase = new Purchase
            {
                Products = _cartItems.Select(p => p.ProductId).ToList(),
                Prices = _cartItems.Select(p => p.Product.Price).ToList(),
                Quantities = _cartItems.Select(p => p.Quantity).ToList(),
                UserId = _userId,
                CouponCode = _couponData?.coupon_code ?? null,
                TotalPrice = _finalPrice,
                TotalPriceBeforeDiscount = _totalPricePreDiscount,
                Date = DateTime.UtcNow
            };

            _dbContext.Purchases.Add(purchase);
            return this;
        }

        public ICartPurchaseBuilder ClearCart()
        {
            _dbContext.Carts.RemoveRange(_cartItems);
            return this;
        }
        public async Task<ICartPurchaseBuilder> PersistAllChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
            return this;
        }
        public async Task<ICartPurchaseBuilder> BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
            return this;
        }

        public async Task<ICartPurchaseBuilder> CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new BadRequestException("No transaction to commit.");

            await _transaction.CommitAsync();
            return this;
        }

        public async Task<ICartPurchaseBuilder> RollbackTransactionAsync()
        {
            if (_transaction == null)
                throw new BadRequestException("No transaction to rollback.");

            await _transaction.RollbackAsync();
            return this;
        }

        public PurchaseResponse? FormatOutput()
        {
            _finalPurchase = new PurchaseResponse
            {
                final_total = _finalPrice,
                total_after_discount = _totalAfterDiscount,
                shipping_cost = _shipping_cost,
                user_id = _userId,
                total_before_discount = _totalPricePreDiscount,
                shopping_cart = _cartItems.Select(c => c.ProductId),
                coupon_applied = _couponData
            };
            return _finalPurchase;
        }
    }

}
