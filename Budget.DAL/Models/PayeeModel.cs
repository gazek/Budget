using System.ComponentModel.DataAnnotations;

namespace Budget.DAL.Models
{
    public class PayeeModel
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

    }
}
