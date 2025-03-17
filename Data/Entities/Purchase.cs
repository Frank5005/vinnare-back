using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Purchase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Column(TypeName = "serial")]
        public int Id { get; set; }

        [Required]
        public List<int> Products { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int CouponApplied { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("UserId")]
        public User User { get; set; }

    }
}
