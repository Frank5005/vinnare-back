using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync();
        Task<InventoryDto?> GetInventoryByIdAsync(int id);
        Task<InventoryDto> CreateInventoryAsync(InventoryDto inventoryDto);

        Task<InventoryDto?> UpdateInventoryAsync(int id, InventoryDto inventoryDto);

        Task<InventoryDto?> DeleteInventoryAsync(int id);
    }
}
