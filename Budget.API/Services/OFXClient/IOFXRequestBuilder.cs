namespace Budget.API.Services.OFXClient
{
    public interface IOFXRequestBuilder
    {
        string Body { get; }
        string SignOn { get; }
        string MsgSet { get; }
        OFXRequestConfig Config { get; }
        string Header { get; }
        string Request { get; }
    }
}