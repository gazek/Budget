using System.Net;

namespace Budget.API.Services.OFXClient
{
    public interface IOFXRequestor
    {
        string ErrorMessage { get; }
        string Header { get; }
        string OFX { get; }
        string Response { get; }
        bool Status { get; }
        HttpStatusCode StatusCode { get; }
        string StatusDescription { get; }

        void Post();
    }
}