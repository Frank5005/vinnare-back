namespace Shared.DTOs
{
    public class PurchaseDto
    {
        public int Id { get; set; }
        public List<int> Products { get; set; }
        public Guid UserId { get; set; }
        public int CouponApplied { get; set; }
        public DateTime Date { get; set; }
    }
}