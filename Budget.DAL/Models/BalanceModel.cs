using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class BalanceModel
    {
        [Key]
        [Display(Name = "Balance ID")]
        public int Id { get; set; }

        [Required]
        [Index("IX_AccountIdAsOfDate", 1, IsUnique = true)]
        public int AccountId { get; set; }

        [Required]
        [Index("IX_AccountIdAsOfDate", 2, IsUnique = true)]
        [Display(Name = "Balance As Of Date")]
        public DateTime AsOfDate { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [ForeignKey("AccountId")]
        [JsonIgnore]
        public virtual AccountModel Account { get; set; }

    }
}
