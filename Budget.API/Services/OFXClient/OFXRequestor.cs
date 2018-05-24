using System.Net;

namespace Budget.API.Services.OFXClient
{
    public class OFXRequestor : IOFXRequestor
    {
        public string Response { get; private set; }
        public string Header { get; private set; }
        public string OFX { get; private set; }
        public bool Status { get; private set; }
        public string ErrorMessage { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string StatusDescription { get; private set; }

        OFXRequestConfig _config;
        IOFXRequestBuilder _requestBuilder;

        public OFXRequestor(IOFXRequestBuilder requestBuilder)
        {
            _requestBuilder = requestBuilder;
            _config = requestBuilder.Config;
            Status = false;
            ErrorMessage = "";
            StatusDescription = "";
        }

        public void Post()
        {
            PostToFinancialInsitution();
            if (Status && Response != null)
            {
                var partitionedResponse = OFXUtils.PartitionResponse(Response);
                Header = partitionedResponse["header"];
                OFX = partitionedResponse["body"];
            }
        }

        void PostToFinancialInsitution()
        {
            ServicePointManager.Expect100Continue = false;
            using (var client = new WebClient())
            {
                client.Headers.Set("Content-Type", "application/x-ofx");
                try
                {
                    Response = client.UploadString(_config.URL.AbsoluteUri, _requestBuilder.Request);
                    Status = true;
                }
                catch(WebException e)
                {
                    Status = false;
                    ErrorMessage = e.Message;
                    if (e.Response != null)
                    {
                        StatusCode = ((HttpWebResponse)e.Response).StatusCode;
                        StatusDescription = ((HttpWebResponse)e.Response).StatusDescription;
                    }
                }
            }
        }
    }
}