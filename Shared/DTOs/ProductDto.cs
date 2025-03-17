namespace Shared.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public bool Approved { get; set; }
    }
}
