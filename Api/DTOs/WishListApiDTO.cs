using System.Text.Json.Serialization;
using Shared.DTOs;

namespace Api.DTOs
{
    public class GetWishListResponse
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
        [JsonPropertyName("wishlist")]
        public IEnumerable<ProductDto>? Products { get; set; }
    }
}