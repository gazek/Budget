using System.Collections.Generic;
using Moq;
using Budget.API.Services.OFXClient;
using Budget.DAL.Models;

namespace Budget.API.Tests.FakesAndMocks
{
    public class MockOfxClient
    {
        public OFXRequestConfig RequestConfig { get; set; }
        public Mock<IOFXRequestBuilder> RequestBuilder { get; set; }
        public Mock<IOFXRequestor> Requestor { get; set; }
        public Mock<IOFXParser> Parser { get; set; }
        public Mock<IOfxClient> Client { get; set; }
        public Mock<IOFXResponseStatus> SignonStatus { get; set; }

        public MockOfxClient()
        {
            // initialize and configure components
            RequestConfig = new OFXRequestConfig();
            ConfigureClient();
            ConfigureRequestBuilder();
            ConfigureRequestor();
            ConfigureParser();
        }

        public IOfxClient GetMock()
        {
            // assemble client
            Client.SetupGet(x => x.RequestConfig).Returns(RequestConfig);
            Client.SetupGet(x => x.RequestBuilder).Returns(RequestBuilder.Object);
            Client.SetupGet(x => x.Requestor).Returns(Requestor.Object);
            Client.SetupGet(x => x.Parser).Returns(Parser.Object);

            // return object
            return Client.Object;
        }

        public MockOfxClient WithFailedRequest()
        {
            Requestor.SetupGet(x => x.Status).Returns(false);
            Requestor.SetupGet(x => x.ErrorMessage).Returns("fake error message");
            return this;
        }

        public MockOfxClient WithSuccessfulRequest(string ofxString)
        {
            Requestor.SetupGet(x => x.Status).Returns(true);
            Requestor.SetupGet(x => x.OFX).Returns(ofxString);
            return this;
        }

        public MockOfxClient WithSignonFailure(string message)
        {
            Parser.Setup(x => x.SignOnRequest.Status).Returns(false);
            Parser.Setup(x => x.SignOnRequest.Message).Returns(message);
            return this;
        }

        public MockOfxClient WithSignonFailer(string message)
        {
            SignonStatus.SetupGet(x => x.Status).Returns(false);
            SignonStatus.SetupGet(x => x.Message).Returns(message);
            return this;
        }

        public MockOfxClient WithAccounts(List<AccountModel> accounts)
        {
            Parser.SetupGet(x => x.Accounts).Returns(accounts);
            return this;
        }

        public MockOfxClient WithSignonSuccess()
        {
            SignonStatus.SetupGet(x => x.Status).Returns(true);
            return this;
        }

        private void ConfigureClient()
        {
            Client = new Mock<IOfxClient>();
        }

        private void ConfigureRequestBuilder()
        {
            RequestBuilder = new Mock<IOFXRequestBuilder>();
        }

        private void ConfigureRequestor()
        {
            Requestor = new Mock<IOFXRequestor>();
            Requestor.Setup(x => x.Post());
        }

        private void ConfigureParser()
        {
            SignonStatus = new Mock<IOFXResponseStatus>();
            Parser = new Mock<IOFXParser>();
            Parser.SetupGet(x => x.SignOnRequest).Returns(SignonStatus.Object);
        }
    }
}
