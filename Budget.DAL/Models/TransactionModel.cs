using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget.DAL.Models
{
    public class TransactionModel
    {
        
        [Required]
        public int Id { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int AccountId { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        // Finncial institution provided transaction reference value/identifier
        public string ReferenceValue { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Key]
        [Column(Order = 3)]
        [Required]
        public DateTime Date { get; set; }


        // Finncial institution provided name of payee
        public string OriginalPayeeName { get; set; }

        // Finncial institution provided memo
        public string OriginalMemo { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        //public IEnumerable<TransactionDetailModel> Details { get; set; }

        [ForeignKey("AccountId")]
        [JsonIgnore]
        public AccountModel Account { get; set; }
    }
}
