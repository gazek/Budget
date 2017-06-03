using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Budget.API.Models
{
    public class CategoryBindingModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression("(?i)^(?!.*uncategorized).*$", ErrorMessage = "Invalid Category name: Category name may not contain the string uncategorized")]
        public string Name { get; set; }

        public IList<SubCategoryBindingModel> SubCategories { get; set; }
    }
}