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
        public int AccountId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        [Display(Name = "Balance As Of Date")]
        public DateTime AsOfDate { get; set; }

        [ForeignKey("AccountId")]
        [JsonIgnore]
        public AccountModel Account { get; set; }

    }
}
