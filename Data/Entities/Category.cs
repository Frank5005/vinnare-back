using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Column(TypeName = "serial")]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool Approved { get; set; }

        //Navigation Property
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Job>? Jobs { get; set; } //= new List<Job>();
    }
}
