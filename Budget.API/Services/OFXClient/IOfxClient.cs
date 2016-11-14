namespace Budget.API.Services.OFXClient
{
    public interface IOfxClient
    {
        IOFXParser Parser { get; set; }
        IOFXRequestBuilder RequestBuilder { get; set; }
        OFXRequestConfig RequestConfig { get; set; }
        IOFXRequestor Requestor { get; set; }

        void BuildRequest();
        void ExecuteRequest();
        void ParseResponse();
    }
}