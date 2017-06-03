using System.ComponentModel.DataAnnotations;

namespace Budget.API.Models
{
    public class PayeeDefaultDetailBindingModel
    {
        public int Id { get; set; }
        [Required]
        public int PayeeId { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int SubCategoryId { get; set; }
        [Required]
        public decimal Allocation { get; set; }
    }
}