namespace Shared.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemDto
    {
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Image { get; set; }
        public int? CategoryId { get; set; }
        public int Available { get; set; }
    }

}