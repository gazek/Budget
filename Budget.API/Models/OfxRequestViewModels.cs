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
            Status = ofxClient.Requestor?.Status ?? false;
            Code = ofxClient.Requestor?.StatusCode ?? HttpStatusCode.BadRequest;
            Response = ofxClient.Requestor?.Response ?? null;
            Description = ofxClient.Requestor?.StatusDescription ?? null;
            Message = new List<string>();
            Message.Add(ofxClient.Requestor?.ErrorMessage ?? null);
            OfxResponse = ofxClient.Requestor?.OFX ?? null;
            MsgSetRequestOfx = ofxClient.RequestBuilder?.MsgSet ?? null;
            SignOn = new OFXReqestResponseViewModel();
            Statement = new OFXReqestResponseViewModel();
            Balance = new OFXReqestResponseViewModel();
        }

        public void PopulateResponse(IOfxClient ofxClient)
        {
            PopulateResponse(ofxClient.Parser);
        }

        public void PopulateResponse(IOFXParser ofxParser)
        {
            Status = ofxParser.SignOnRequest.Status && ofxParser.StatementRequest.Status && ofxParser.BalanceRequest.Status;
            SignOn.Status = ofxParser.SignOnRequest.Status;
            SignOn.Code = ofxParser.SignOnRequest.Code;
            SignOn.Severity = ofxParser.SignOnRequest.Severity;
            SignOn.Message = ofxParser.SignOnRequest.Message;
            Statement.Status = ofxParser.StatementRequest.Status;
            Statement.Code = ofxParser.StatementRequest.Code;
            Statement.Severity = ofxParser.StatementRequest.Severity;
            Statement.Message = ofxParser.StatementRequest.Message;
            Balance.Status = ofxParser.BalanceRequest.Status;
            Balance.Code = ofxParser.BalanceRequest.Code;
            Balance.Severity = ofxParser.BalanceRequest.Severity;
            Balance.Message = ofxParser.BalanceRequest.Message;
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