using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Budget.API.Services
{
    public class OFXStatementRequestor
    {
        public string RequestStatus
        {
            get { return _status; }
        }
        public string Response
        {
            get { return _response; }
        }
        OFXStatementRequestConfig _config;
        OFXStatementRequestBuilder _requestBuilder;
        private string _response;
        private string _status;

        public OFXStatementRequestor(OFXStatementRequestConfig config)
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