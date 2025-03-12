using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class InventoryService : IInventoryService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(VinnareDbContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync()
        {
            return await _context.Inventories
                .Select(i => new InventoryDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Total = i.Total,
                    Available = i.Available
                })
                .ToListAsync();
        }

        public async Task<InventoryDto?> GetInventoryByIdAsync(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return null;

            return new InventoryDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                Total = inventory.Total,
                Available = inventory.Available
            };
        }

        public async Task<InventoryDto> CreateInventoryAsync(InventoryDto inventoryDto)
        {
            var inventory = new Inventory
            {
                ProductId = inventoryDto.ProductId,
                Total = inventoryDto.Total,
                Available = inventoryDto.Available
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return inventoryDto;
        }

        public async Task<InventoryDto?> UpdateInventoryAsync(int id, InventoryDto inventoryDto)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return null;

            inventory.ProductId = inventoryDto.ProductId;
            inventory.Total = inventoryDto.Total;
            inventory.Available = inventoryDto.Available;

            await _context.SaveChangesAsync();

            return inventoryDto;
        }

        public async Task<InventoryDto?> DeleteInventoryAsync(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return null;

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return new InventoryDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                Total = inventory.Total,
                Available = inventory.Available
            };
        }
    }
}
