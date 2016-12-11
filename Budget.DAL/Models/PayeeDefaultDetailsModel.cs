using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class PayeeDefaultDetailsModel
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Index("IX_PayeeCatSubCatIds", 1, IsUnique = true)]
        [Required]
        public int PayeeId { get; set; }

        [Index("IX_PayeeCatSubCatIds", 2, IsUnique = true)]
        [Required]
        public int CategoryId { get; set; }

        [Index("IX_PayeeCatSubCatIds", 3, IsUnique = true)]
        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal Allocation { get; set; }

        [ForeignKey("PayeeId")]
        [JsonIgnore]
        public virtual PayeeModel Payee { get; set; }

        [ForeignKey("CategoryId")]
        [JsonIgnore]
        public virtual CategoryModel Category { get; set; }

        [ForeignKey("SubCategoryId")]
        [JsonIgnore]
        public virtual SubCategoryModel SubCategory { get; set; }
    }
}
