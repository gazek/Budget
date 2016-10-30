using System.ComponentModel.DataAnnotations;

namespace Budget.DAL.Models
{
    public class CategoryModel
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

    }
}
