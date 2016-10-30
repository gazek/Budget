using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class SubCategoryModel
    {
        [Key]
        [Column(Order = 1)]
        [Required]
        public int Id { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("CategoryId")]
        [JsonIgnore]
        public virtual CategoryModel Category { get; set; }
    }
}
