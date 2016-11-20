using System;
using System.Reflection;

namespace Budget.API.Services.OFXClient
{
    public class OFXRequestConfig
    {
        public OFXRequestConfigRequestType RequestType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string OfxOrg { get; set; }
        public int OfxFid { get; set; }
        public int RoutingNumber { get; set; }
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