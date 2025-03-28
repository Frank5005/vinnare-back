using Shared.DTOs;

namespace Services.Builders
{

    public interface ICartPurchaseBuilder
    {
        Task<ICartPurchaseBuilder> BeginTransactionAsync();

        Task<ICartPurchaseBuilder> LoadCartAsync();

        ICartPurchaseBuilder ValidateApproved();
        ICartPurchaseBuilder ValidateStock();
        ICartPurchaseBuilder CalcBasePrice();

        Task<ICartPurchaseBuilder> FindCoupon(string? coupon_code);

        ICartPurchaseBuilder CalcFinalPrice();

        ICartPurchaseBuilder DecrementStock();
        ICartPurchaseBuilder CreatePurchase();
        ICartPurchaseBuilder ClearCart();

        Task<ICartPurchaseBuilder> PersistAllChangesAsync();

        Task<ICartPurchaseBuilder> CommitTransactionAsync();

        Task<ICartPurchaseBuilder> RollbackTransactionAsync();

        PurchaseResponse? FormatOutput();
    }
}
