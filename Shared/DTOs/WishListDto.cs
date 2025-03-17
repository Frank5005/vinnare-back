namespace Shared.DTOs
{
    public class WishListDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int ProductId { get; set; }
    }
}