using Budget.API.Services.OFXClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Budget.Tests.Services
{
    [TestClass]
    public class OFXRequestorTests
    {
        [TestMethod]
        public void OFXRequestorPostTest()
        {
            // Arrange
            OFXRequestConfig config = new OFXRequestConfig
            {
                RequestType = OFXRequestConfigRequestType.Statement,
                Username = "fakeuser",
                Password = "fakepass",
                URL = new Uri("https://ofx.chase.com"),
                OfxOrg = "B1",
                OfxFid = 10898,
                AccountNumber = "1234567890",
                AccountType = OFXRequestConfigAccountType.SAVINGS,
                StartDate = new DateTime(2016, 9, 1),
                EndDate = new DateTime(2016, 9, 30),
            };
            OFXRequestBuilder requestBuilder = new OFXRequestBuilder(config);
            OFXRequestor request = new OFXRequestor(requestBuilder);
            string expectedHeader = "OFXHEADER:";
            string expectedOfx = "<OFX><SIGNONMSGSRSV1><SONRS><STATUS><CODE>";

            // Act
            request.Post();

            // Assert
            Assert.IsTrue(request.Status);
            Assert.IsTrue(request.Header.Replace("\r\n ", string.Empty).IndexOf(expectedHeader) >= 0);
            Assert.IsTrue(request.OFX.IndexOf(expectedOfx) >= 0);
        }
    }
}
