namespace Budget.API.Models
{
    public class PayeeDefaultDetailViewModel
    {
        public int Id { get; set; }
        public int PayeeId { get; set; }
        public string PayeeName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public decimal Allocation { get; set; }
    }
}