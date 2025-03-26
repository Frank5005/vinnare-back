namespace Api.DTOs
{
    public class CouponRequest
    {
        public string code { get; set; }
        public int discountPercentage { get; set; }
    }
}
