using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class BalanceModel
    {
        [Required]
        public int Id { get; set; }

        [Key]
        [Required]
        public int AccountId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Key]
        [Required]
        [Display(Name = "Balance As Of Date")]
        public DateTime AsOfDate { get; set; }

        [ForeignKey("AccountId")]
        [JsonIgnore]
        public AccountModel Account { get; set; }

    }
}
