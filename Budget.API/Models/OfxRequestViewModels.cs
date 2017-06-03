using System.Net;

namespace Budget.API.Models
{
    public class OfxTransactionRequestViewModel
    {
        public bool Status { get; set; }
        public HttpStatusCode Code { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public string Description { get; set; }
        public string OfxResponse { get; set; }
        public OFXReqestResponseViewModel SignOn { get; set; }
        public OFXReqestResponseViewModel Statement { get; set; }
        public OFXReqestResponseViewModel Balance { get; set; }
        public OFXReqestResponseViewModel AccountList { get; set; }
        public string MsgSetRequestOfx { get; set; }
    }

    public class OFXReqestResponseViewModel
    {
        public bool Status { get; set; }
        public int Code { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }

        public OFXReqestResponseViewModel()
        {
            Status = false;
            Code = -1;
        }
    }
}