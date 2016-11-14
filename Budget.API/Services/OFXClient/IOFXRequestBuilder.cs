namespace Budget.API.Services.OFXClient
{
    public interface IOFXRequestBuilder
    {
        string Body { get; }
        OFXRequestConfig Config { get; }
        string Header { get; }
        string Request { get; }
    }
}