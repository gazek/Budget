namespace Budget.API.Services.OFXClient
{
    public class OfxClient
    {
        public OFXRequestConfig RequestConfig { get; set; }
        public OFXRequestBuilder RequestBuilder { get; set; }
        public OFXRequestor Requestor { get; set; }
        public OFXParser Parser { get; set; }

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