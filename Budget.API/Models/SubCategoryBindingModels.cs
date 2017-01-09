using System.ComponentModel.DataAnnotations;

namespace Budget.API.Models
{
    public class SubCategoryBindingModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}