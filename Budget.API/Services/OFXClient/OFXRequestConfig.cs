using System;
using System.Reflection;

namespace Budget.API.Services.OFXClient
{
    public class OFXRequestConfig
    {
        public OFXRequestConfigRequestType RequestType { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string InstitutionName { get; set; }
        public int InstitutionId { get; set; }
        public int InstitutionRoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public OFXRequestConfigAccountType AccountType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Uri URL { get; set; }
        public Guid ClientUID { get; set; }

        public OFXRequestConfig()
        {
            // set default value
            ClientUID = new Guid();
        }
        
    }

    public enum OFXRequestConfigRequestType
    {
        SignOn,
        Statement,
        AccountList,
        Balance
    }

    public enum OFXRequestConfigAccountType
    {
        SAVINGS,
        CHECKING,
        CREDITCARD
    }
}