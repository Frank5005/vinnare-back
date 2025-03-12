namespace Shared.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }

        public int ProductId { get; set; }

        public string Comment { get; set; }
        public int Rate { get; set; }
    }
}
