using System.Net;

namespace Budget.API.Services.OFXClient
{
    public class OFXRequestor
    {
        public string Response { get { return _response; } }
        private string _response;

        public string Header { get { return _header; } }
        private string _header;

        public string OFX  {  get { return _ofx; } }
        private string _ofx;

        public bool Status { get { return _status; } }
        private bool _status;

        public string ErrorMessage { get { return _errorMessage; } }
        private string _errorMessage;

        public HttpStatusCode StatusCode { get { return _statusCode; } }
        private HttpStatusCode _statusCode;

        public string StatusDescription { get { return _statusDescription; } }
        private string _statusDescription;

        OFXRequestConfig _config;
        OFXRequestBuilder _requestBuilder;

        public OFXRequestor(OFXRequestBuilder requestBuilder)
        {
            _requestBuilder = requestBuilder;
            _config = requestBuilder.Config;
            _status = false;
        }

        public void Post()
        {
            PostToFinancialInsitution();
            if (_status && _response != null)
            {
                PartitionResponse();
            }
        }

        void PostToFinancialInsitution()
        {
            ServicePointManager.Expect100Continue = false;
            using (var client = new WebClient())
            {
                client.BaseAddress = _config.URL.AbsoluteUri;
                client.Headers.Set("Content-Type", "application/x-ofx");
                try
                {
                    _response = client.UploadString("", _requestBuilder.Request);
                    _status = true;
                }
                catch(WebException e)
                {
                    _status = false;
                    _errorMessage = e.Message;
                    _statusCode = ((HttpWebResponse)e.Response).StatusCode;
                    _statusDescription = ((HttpWebResponse)e.Response).StatusDescription;
                }
            }
        }

        void PartitionResponse()
        {
            int ofxStartIndex = _response.IndexOf("<OFX>");
            _header = _response.Substring(0, ofxStartIndex);
            _ofx = _response.Substring(ofxStartIndex);
        }
    }
}