using Shared.Enums;

namespace Shared.DTOs
{
    public class PurchaseDto
    {
        public int Id { get; set; }
        public List<int> Products { get; set; } = new();
        public List<decimal> Prices { get; set; } = new();
        public List<int> Quantities { get; set; } = new();
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? CouponCode { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPriceBeforeDiscount { get; set; }
        public DateTime Date { get; set; }
        public string Address { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "paid";
        public string Status { get; set; } = "pending";
    }

    public class CouponData
    {
        public string coupon_code { get; set; }
        public int discount_percentage { get; set; }
    }
    public class PurchaseResponse
    {
        public Guid user_id { get; set; }
        public string user_name { get; set; } = string.Empty;
        public IEnumerable<int> shopping_cart { get; set; }
        public CouponData? coupon_applied { get; set; }
        public decimal total_before_discount { get; set; }
        public decimal total_after_discount { get; set; }
        public decimal shipping_cost { get; set; }
        public decimal final_total { get; set; }
        public string address { get; set; } = string.Empty;
        public string payment_status { get; set; } = "paid";
        public string status { get; set; } = "pending";
    }
}