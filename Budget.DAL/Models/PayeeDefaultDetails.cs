using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class PayeeDefaultDetails
    {
        [Key]
        [Column(Order = 1)]
        [Required]
        public int PayeeId { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        public int CategoryId { get; set; }

        [Key]
        [Column(Order = 3)]
        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        [Range(-100, 100)]
        public int Allocation { get; set; }
    }
}
