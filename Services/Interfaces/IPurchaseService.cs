using Shared.DTOs;

namespace Services.Interfaces

{
    public interface IPurchaseService
    {
        Task<IEnumerable<PurchaseDto>> GetAllPurchasesAsync();
        Task<PurchaseDto?> GetPurchaseByIdAsync(int id);
        Task<PurchaseDto> CreatePurchaseAsync(PurchaseDto purchaseDto);

        Task<PurchaseDto?> UpdatePurchaseAsync(int id, PurchaseDto purchaseDto);

        Task<PurchaseDto?> DeletePurchaseAsync(int id);
    }
}