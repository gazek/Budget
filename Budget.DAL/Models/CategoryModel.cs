using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class CategoryModel
    {
        [Key]
        [Required]
        [Index(IsUnique = true)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public IEnumerable<SubCategoryModel> SubCategories { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual IdentityUser User { get; set; }
    }
}
