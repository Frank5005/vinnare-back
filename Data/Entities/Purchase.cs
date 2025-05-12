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
        public List<decimal> Prices { get; set; }
        [Required]
        public List<int> Quantities { get; set; }
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        public string? CouponCode { get; set; }

        [Required]
        [Column(TypeName = "money")]
        public decimal TotalPrice { get; set; }

        [Required]
        [Column(TypeName = "money")]
        public decimal TotalPriceBeforeDiscount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string PaymentStatus { get; set; } = "paid";

        [Required]
        public string Status { get; set; } = "pending";

        // Navigation Property
        [ForeignKey("UserId")]
        public User User { get; set; }

    }
}
