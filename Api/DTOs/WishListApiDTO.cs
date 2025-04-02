using System.Text.Json.Serialization;

namespace Api.DTOs
{
    public class GetWishListResponse
    {
        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
        [JsonPropertyName("wishlist")]
        public IEnumerable<int>? Products { get; set; }
    }
}