using Budget.API.Services.OFXClient;
using System.Collections.Generic;
using System.Net;

namespace Budget.API.Models
{
    public class OfxTransactionRequestViewModel
    {
        public bool Status { get; set; }
        public HttpStatusCode Code { get; set; }
        public List<string> Message { get; set; }
        public string Response { get; set; }
        public string Description { get; set; }
        public string OfxResponse { get; set; }
        public OFXReqestResponseViewModel SignOn { get; set; }
        public OFXReqestResponseViewModel Statement { get; set; }
        public OFXReqestResponseViewModel Balance { get; set; }
        public string MsgSetRequestOfx { get; set; }

        public OfxTransactionRequestViewModel()
        {
            Message = new List<string>();
        }

        public OfxTransactionRequestViewModel(IOfxClient ofxClient)
        {
            Status = ofxClient.Requestor.Status;
            Code = ofxClient.Requestor.StatusCode;
            Response = ofxClient.Requestor.Response;
            Description = ofxClient.Requestor.StatusDescription;
            Message = new List<string>();
            Message.Add(ofxClient.Requestor.ErrorMessage);
            OfxResponse = ofxClient.Requestor.OFX;
            MsgSetRequestOfx = ofxClient.RequestBuilder.MsgSet;
            SignOn = new OFXReqestResponseViewModel();
            Statement = new OFXReqestResponseViewModel();
            Balance = new OFXReqestResponseViewModel();
        }

        public void PopulateResponse(IOfxClient ofxClient)
        {
            Status = ofxClient.Parser.SignOnRequest.Status && ofxClient.Parser.StatmentRequest.Status && ofxClient.Parser.BalanceRequest.Status;
            SignOn.Status = ofxClient.Parser.SignOnRequest.Status;
            SignOn.Code = ofxClient.Parser.SignOnRequest.Code;
            SignOn.Severity = ofxClient.Parser.SignOnRequest.Severity;
            SignOn.Message = ofxClient.Parser.SignOnRequest.Message;
            Statement.Status = ofxClient.Parser.StatmentRequest.Status;
            Statement.Code = ofxClient.Parser.StatmentRequest.Code;
            Statement.Severity = ofxClient.Parser.StatmentRequest.Severity;
            Statement.Message = ofxClient.Parser.StatmentRequest.Message;
            Balance.Status = ofxClient.Parser.BalanceRequest.Status;
            Balance.Code = ofxClient.Parser.BalanceRequest.Code;
            Balance.Severity = ofxClient.Parser.BalanceRequest.Severity;
            Balance.Message = ofxClient.Parser.BalanceRequest.Message;
        }
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