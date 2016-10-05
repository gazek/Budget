using Budget.API.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget.Tests.Services
{
    [TestClass]
    public class OFXStatementRequestorTest
    {
        [TestMethod]
        public void OFXStatementRequestorPostTest()
        {
            // Arrange
            OFXStatementRequestConfig config = new OFXStatementRequestConfig
            {
                UserId = "fakeuser",
                password = "fakepass!",
                InstitutionName = "First Tech Federal Credit Union",
                InstitutionId = 3169,
                InstitutionRoutingNumber = 321180379,
                AccountNumber = "1234567890",
                AccountType = OFXRequestBuilderConfigAccountType.SAVINGS,
                StartDate = new DateTime(2016, 9, 1),
                EndDate = new DateTime(2016, 9, 30),
                URL = new Uri("https://ofx.firsttechfed.com"),
                IncludeTransactions = true
            };
            OFXStatementRequestor request = new OFXStatementRequestor(config);

            // Act
            request.Post();

            // Assert
            Assert.IsNotNull(request.Response);

        }
    }
}
