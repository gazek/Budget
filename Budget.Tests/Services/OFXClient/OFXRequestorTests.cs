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
                OfxOrg = "First Tech Federal Credit Union",
                OfxFid = 3169,
                RoutingNumber = 321180379,
                AccountNumber = "1234567890",
                AccountType = OFXRequestConfigAccountType.SAVINGS,
                StartDate = new DateTime(2016, 9, 1),
                EndDate = new DateTime(2016, 9, 30),
                URL = new Uri("https://ofx.firsttechfed.com"),
                //ClientUID = new Guid("94f92863-15c1-4874-9fe5-0c84351ac0c2")
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
