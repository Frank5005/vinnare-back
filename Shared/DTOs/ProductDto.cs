namespace Shared.DTOs
{

    public class ProductRequest
    {
        public Guid OwnerId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int Quantity { get; set; }
        public int Available { get; set; }
    }

    public class ProductResponse
    {
        public int Id { get; set; }
        public string message { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool Approved { get; set; }
        public int Quantity { get; set; }
        public int Available { get; set; }
    }
}
