using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Inventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "serial")]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Total { get; set; }

        [Required]
        public int Available { get; set; }

        // Navigation Property
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
