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

        [Required]
        public ActionType Action { get; set; }

        public Guid CreatorId { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("CreatorId")]
        public User User { get; set; }
    }
}
