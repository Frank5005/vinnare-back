using System.Dynamic;

namespace Shared.DTOs
{
    public class ReviewRequest
    {
        public string Username { get; set;}
        public int ProductId { get; set; }
        public string Comment { get; set; }
        public int Rate { get; set; }
    }

    public class ReviewResponse
    {
        public string Username { get; set; }
        public int ProductId { get; set; }
        public string Comment { get; set; }
        //public int Rate { get; set; }
    }
    
    public class ReviewDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }

        public int ProductId { get; set; }

        public string Comment { get; set; }
        public int Rate { get; set; }
    }
}
