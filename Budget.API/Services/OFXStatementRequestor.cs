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
        string RequestStatus
        {
            get { return _response; }
        }
        OFXStatementRequestConfig _config;
        OFXStatementRequestBuilder _requestBuilder;
        private string _response;

        public OFXStatementRequestor(OFXStatementRequestConfig config)
        {
            _config = config;
            _requestBuilder = new OFXStatementRequestBuilder(_config);
        }

        void PostToFinancialInsitution()
        {
            using (var client = new WebClient())
            {
                client.BaseAddress = _config.URL.AbsoluteUri;
                var address = new Uri("");
                _response = client.UploadString(address, _requestBuilder.Request);
            }
        }
    }
}