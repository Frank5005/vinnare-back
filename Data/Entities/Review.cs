using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Column(TypeName = "serial")]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public int ProductId { get; set; }

        public string Comment { get; set; } = string.Empty;

        [Required]
        public int Rate { get; set; } = 0;
        [Required]
        public int Rate { get; set; } = 0;


        // Navigation Property
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
