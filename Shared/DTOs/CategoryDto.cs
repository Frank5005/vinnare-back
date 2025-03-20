namespace Shared.DTOs
{
    public class CategoryRequest
    {
        public string Name { get; set; }
    }

    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Message { get; set; }
    }

    public class CategoryView
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CategoryUpdated
    {
        public string? Name { get; set; }
        public bool? Approved { get; set; }
    }

    public class CategoryDelete
    {
        public string Message { get; set; }
    }
    
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Approved { get; set; }
    }
}