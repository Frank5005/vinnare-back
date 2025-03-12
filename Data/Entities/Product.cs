using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        public string? Image { get; set; }

        public bool Approved { get; set; } = false;

        // Navigation Property
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        [ForeignKey("Category")]
        public Category Categori { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
