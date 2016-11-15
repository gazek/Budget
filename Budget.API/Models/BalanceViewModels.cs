using System;

namespace Budget.API.Models
{
    public class BalanceViewModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime AsOfDate { get; set; }
        public decimal Amount { get; set; }
    }
}