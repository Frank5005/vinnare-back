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
        public string Username { get; set; }
    }

    public class ProductResponse
    {
        public int Id { get; set; }
        public string message { get; set; }
    }

    public class ProductViewPage
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
        public int Rate {get; set; }
        public int Quantity { get; set; }
        public int Available { get; set; }

    }

    public class ProductDetail
    {
        public int Id { get; set; }
        public string Owner { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
        public int Rate {get; set; }
        public int Quantity { get; set; }
        public int Available { get; set; }
    }

    public class ProductView
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }

    public class ProductUpdate
    {
        public Guid? OwnerId { get; set; }
        public string? Title { get; set; }
        public decimal? Price { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        //public bool? Approved { get; set; }
        public int? Quantity { get; set; }
        public int? Available { get; set; }
    }

    public class ProductDelete
    {
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
        public DateTime Date { get; set; }
    }
}
