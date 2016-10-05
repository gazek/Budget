using Budget.API.Services.OFXClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Budget.Tests.Services
{
    [TestClass]
    public class OFXRequestorTest
    {
        [TestMethod]
        public void OFXRequestorPostTest()
        {
            // Arrange
            OFXRequestConfig config = new OFXRequestConfig
            {
                RequestType = OFXRequestConfigRequestType.Statement,
                UserId = "fakeuser",
                password = "fakepass!",
                InstitutionName = "First Tech Federal Credit Union",
                InstitutionId = 3169,
                InstitutionRoutingNumber = 321180379,
                AccountNumber = "1234567890",
                AccountType = OFXRequestConfigAccountType.SAVINGS,
                StartDate = new DateTime(2016, 9, 1),
                EndDate = new DateTime(2016, 9, 30),
                URL = new Uri("https://ofx.firsttechfed.com"),
                IncludeTransactions = true
            };
            OFXRequestor request = new OFXRequestor(config);

            // Act
            request.Post();

            // Assert
            Assert.IsNotNull(request.Response);

        }
    }
}
