using System.ComponentModel.DataAnnotations;

namespace Budget.API.Models
{
    public class SubCategoryBindingModel
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression("(?i)^(?!.*uncategorized).*$", ErrorMessage = "Invalid SubCategory name: SubCategory name may not contain the string uncategorized")]
        public string Name { get; set; }
    }
}