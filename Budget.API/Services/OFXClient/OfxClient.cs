namespace Budget.API.Services.OFXClient
{
    public class OfxClient : IOfxClient
    {
        public OFXRequestConfig RequestConfig { get; set; }
        public IOFXRequestBuilder RequestBuilder { get; set; }
        public IOFXRequestor Requestor { get; set; }
        public IOFXParser Parser { get; set; }

        public OfxClient()
        {
            RequestConfig = new OFXRequestConfig();
        }

        public OfxClient(OFXRequestConfig config)
        {
            RequestConfig = config;
        }

        public void BuildRequest()
        {
            RequestBuilder = new OFXRequestBuilder(RequestConfig);
        }

        public void ExecuteRequest()
        {
            Requestor = new OFXRequestor(RequestBuilder);
            Requestor.Post();
        }

        public void ParseResponse()
        {
            Parser = new OFXParser(Requestor.OFX);
            Parser.Parse();
        }
    }
}