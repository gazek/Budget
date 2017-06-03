using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.API.Models
{
    public class BalanceBindingModel
    {
        [ForeignKey("Account")]
        [Required]
        public int AccountId { get; set; }

        [Required]
        [Display(Name = "Balance As Of Date")]
        public DateTime AsOfDate { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}
