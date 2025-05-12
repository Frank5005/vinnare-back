using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;

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

        public async Task<IEnumerable<PurchaseDto>> GetAllUserPurchases(Guid id)
        {
            return await _context.Purchases
                .Where(p => p.UserId == id)
                .Include(p => p.User)
                .Select(p => new PurchaseDto
                {
                    Id = p.Id,
                    Products = p.Products,
                    Prices = p.Prices,
                    Quantities = p.Quantities,
                    UserId = p.UserId,
                    UserName = p.User.Name,
                    Address = p.Address,    
                    TotalPrice = p.TotalPrice,
                    TotalPriceBeforeDiscount = p.TotalPriceBeforeDiscount,
                    Date = p.Date,
                    PaymentStatus = p.PaymentStatus,
                    Status = p.Status
                })
                .ToListAsync();
        }

    }
}
