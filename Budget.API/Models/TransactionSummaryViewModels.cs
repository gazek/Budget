namespace Budget.API.Models
{
    public class TransactionSummaryViewModel
    {
        public int NewCount { get; set; }
        public int AttentionCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public int VoidCount { get; set; }
    }
}