using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class CategoryModel
    {
        [Key]
        [Display(Name = "Category ID")]
        public int Id { get; set; }

        [Required]
        [Index("IX_NameUsderId", 1, IsUnique = true)]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Index("IX_NameUsderId", 2, IsUnique = true)]
        public string UserId { get; set; }

        public ICollection<SubCategoryModel> SubCategories { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }
    }
}
