using System.Text.Json.Serialization;

namespace Shared.DTOs
{
    public class WishListDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int ProductId { get; set; }

    }

    public class CreateWishListRequest
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }
    }
}