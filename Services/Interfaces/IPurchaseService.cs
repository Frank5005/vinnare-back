using Shared.DTOs;

namespace Services.Interfaces

{
    public interface IPurchaseService
    {
        Task<IEnumerable<PurchaseDto>> GetAllUserPurchases(Guid id);
    }
}