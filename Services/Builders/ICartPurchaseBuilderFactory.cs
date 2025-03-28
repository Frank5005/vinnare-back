namespace Services.Builders
{
    public interface ICartPurchaseBuilderFactory
    {
        ICartPurchaseBuilder Create(Guid userId);
    }

}
