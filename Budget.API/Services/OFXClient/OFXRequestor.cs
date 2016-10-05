using System.Net;

namespace Budget.API.Services.OFXClient
{
    public class OFXRequestor
    {
        public string RequestStatus
        {
            get { return _status; }
        }
        public string Response
        {
            get { return _response; }
        }
        OFXRequestConfig _config;
        OFXStatementRequestBuilder _requestBuilder;
        private string _response;
        private string _status;

        public OFXRequestor(OFXRequestConfig config)
        {
            _config = config;
            _requestBuilder = new OFXStatementRequestBuilder(_config);
        }

        public void Post()
        {
            this.PostToFinancialInsitution();
        }

        void PostToFinancialInsitution()
        {
            ServicePointManager.Expect100Continue = false;
            using (var client = new WebClient())
            {
                client.BaseAddress = _config.URL.AbsoluteUri;
                client.Headers.Set("Content-Type", "application/x-ofx");
                _response = client.UploadString("", _requestBuilder.Request);
            }
        }
    }
}