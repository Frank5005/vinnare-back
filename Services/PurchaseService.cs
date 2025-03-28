using Data;
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

        public async Task<IEnumerable<PurchaseDto>> GetAllUserPurchases(Guid id)
        {
            return await _context.Purchases
                .Where(p => p.UserId == id)
                .Select(p => new PurchaseDto
                {
                    Id = p.Id,
                    Products = p.Products,
                    UserId = p.UserId,
                    Date = p.Date
                })
                .ToListAsync();
        }

    }
}
