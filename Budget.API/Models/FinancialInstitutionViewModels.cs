namespace Budget.API.Models
{
    public class FinancialInstitutionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OfxFid { get; set; }
        public string OfxUrl { get; set; }
        public string OfxOrg { get; set; }
        public string Username { get; set; }

        // User override of default value or force inclusion of OFX Fields
        public string CLIENTUID { get; set; }
    }
}
