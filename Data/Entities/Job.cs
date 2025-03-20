using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Enums;

namespace Data.Entities
{
    public class Job
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Column(TypeName = "serial")]
        public int Id { get; set; }

        [Required]
        public JobType Type { get; set; }

        [Required]
        public OperationType Operation { get; set; }

        public Guid CreatorId { get; set; }

        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("CreatorId")]
        public User User { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

    }
}
