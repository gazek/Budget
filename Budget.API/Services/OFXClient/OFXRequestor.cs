using System.Net;

namespace Budget.API.Services.OFXClient
{
    public class OFXRequestor
    {
        public string Response
        {
            get { return _response; }
        }

        public string Header
        {
            get { return _header; }
        }

        public string OFX
        {
            get { return _ofx; }
        }

        public bool Status
        {
            get { return _status; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        public string StatusDescription
        {
            get { return _statusDescription; }
        }

        OFXRequestConfig _config;
        OFXStatementRequestBuilder _requestBuilder;
        private string _response;
        private string _header;
        private string _ofx;
        private bool _status;
        private HttpStatusCode _statusCode;
        private string _statusDescription;
        private string _errorMessage;

        public OFXRequestor(OFXRequestConfig config)
        {
            _config = config;
            _requestBuilder = new OFXStatementRequestBuilder(_config);
            _status = false;
        }

        public void Post()
        {
            this.PostToFinancialInsitution();
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