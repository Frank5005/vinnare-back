namespace Shared.DTOs
{
    public class PurchaseDto
    {
        public int Id { get; set; }
        public List<int> Products { get; set; } = new();
        public List<decimal> Prices { get; set; } = new();
        public List<decimal> Quantities { get; set; } = new();
        public Guid UserId { get; set; }
        public string? CouponCode { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPriceBeforeDiscount { get; set; }
        public DateTime Date { get; set; }
    }

    public class CouponData
    {
        public string coupon_code { get; set; }
        public int discount_percentage { get; set; }
    }
    public class PurchaseResponse
    {
        public Guid user_id { get; set; }
        public IEnumerable<int> shopping_cart { get; set; }
        public CouponData? coupon_applied { get; set; }
        public decimal total_before_discount { get; set; }
        public decimal total_after_discount { get; set; }
        public decimal shipping_cost { get; set; }
        public decimal final_total { get; set; }
    }
}