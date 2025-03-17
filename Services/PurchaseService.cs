using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(VinnareDbContext context, ILogger<PurchaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<PurchaseDto>> GetAllPurchasesAsync()
        {
            _logger.LogInformation("TESING");
            return await _context.Purchases
                .Select(p => new PurchaseDto
                {
                    Id = p.Id,
                    Products = p.Products,
                    UserId = p.UserId,
                    CouponApplied = p.CouponApplied,
                    Date = p.Date
                })
                .ToListAsync();
        }

        public async Task<PurchaseDto?> GetPurchaseByIdAsync(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null) return null;

            return new PurchaseDto
            {
                Id = purchase.Id,
                Products = purchase.Products,
                UserId = purchase.UserId,
                CouponApplied = purchase.CouponApplied,
                Date = purchase.Date
            };
        }

        public async Task<PurchaseDto> CreatePurchaseAsync(PurchaseDto purchaseDto)
        {
            var purchase = new Purchase
            {
                Products = purchaseDto.Products,
                UserId = purchaseDto.UserId,
                CouponApplied = purchaseDto.CouponApplied,
                Date = purchaseDto.Date
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return new PurchaseDto
            {
                Id = purchase.Id,
                Products = purchase.Products,
                UserId = purchase.UserId,
                CouponApplied = purchase.CouponApplied,
                Date = purchase.Date
            };
        }

        public async Task<PurchaseDto?> UpdatePurchaseAsync(int id, PurchaseDto purchaseDto)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null) return null;

            purchase.Products = purchaseDto.Products;
            purchase.UserId = purchaseDto.UserId;
            purchase.CouponApplied = purchaseDto.CouponApplied;
            purchase.Date = purchaseDto.Date;

            await _context.SaveChangesAsync();

            return new PurchaseDto
            {
                Id = purchase.Id,
                Products = purchase.Products,
                UserId = purchase.UserId,
                CouponApplied = purchase.CouponApplied,
                Date = purchase.Date
            };
        }

        public async Task<PurchaseDto?> DeletePurchaseAsync(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null) return null;

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();

            return new PurchaseDto
            {
                Id = purchase.Id,
                Products = purchase.Products,
                UserId = purchase.UserId,
                CouponApplied = purchase.CouponApplied,
                Date = purchase.Date
            };
        }
    }
}
