using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class BalanceModel
    {
        [Key]
        [Column(Order = 1)]
        [Required]
        [Index(IsUnique = true)]
        public int AccountId { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        [Display(Name = "Balance As Of Date")]
        public DateTime AsOfDate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [ForeignKey("AccountId")]
        [JsonIgnore]
        public AccountModel Account { get; set; }

    }
}
