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
            string expectedHeader = "OFXHEADER:100 DATA:OFXSGML VERSION:102 SECURITY:NONE ENCODING:USASCII CHARSET:1252 COMPRESSION:NONE OLDFILEUID:NONE NEWFILEUID:NONE\r\nDATA:OFXSGML\r\nVERSION:102\r\nSECURITY:NONE\r\nENCODING:USASCII\r\nCHARSET:1252\r\nCOMPRESSION:NONE\r\nOLDFILEUID:NONE\r\nNEWFILEUID:NONE\r\n\r\n";
            string expectedOfx = "<OFX><SIGNONMSGSRSV1><SONRS><STATUS><CODE>15500<SEVERITY>ERROR<MESSAGE>User or Member password invalid</STATUS><DTSERVER>20161005152847.025[-7:PDT]<LANGUAGE>ENG<FI><ORG>First Tech Federal Credit Union<FID>3169</FI></SONRS></SIGNONMSGSRSV1><BANKMSGSRSV1><STMTTRNRS><TRNUID>1001<STATUS><CODE>15500<SEVERITY>ERROR</STATUS></STMTTRNRS></BANKMSGSRSV1></OFX>";
            string expected = expectedHeader + expectedOfx;
            int indexOfxL = expectedOfx.IndexOf("<DTSERVER>");
            int indexOfxR = expectedOfx.IndexOf("<LANGUAGE>");
            int indexL = expected.IndexOf("<DTSERVER>");
            int indexR = expected.IndexOf("<LANGUAGE>");

            // Act
            request.Post();

            // Assert
            Assert.AreEqual(request.Header.Replace("\r\n ", string.Empty), expectedHeader.Replace("\r\n ", string.Empty));
            Assert.AreEqual(request.OFX.Substring(0, indexOfxL), expectedOfx.Substring(0, indexOfxL));
            Assert.AreEqual(request.OFX.Substring(indexOfxR), expectedOfx.Substring(indexOfxR));
            Assert.AreEqual(request.Response.Substring(0, indexL), expected.Substring(0, indexL));
            Assert.AreEqual(request.Response.Substring(indexR), expected.Substring(indexR));
        }
    }
}
