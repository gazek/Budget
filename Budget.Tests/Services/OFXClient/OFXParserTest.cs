using Budget.API.Services.OFXClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Budget.Tests.Services.OFXClient
{
    [TestClass]
    public class OFXParserTest
    {
        [TestMethod]
        public void Sandbox()
        {
            // Arrange
            string ofx = "<OFX><SIGNONMSGSRSV1><SONRS><STATUS><CODE>15500<SEVERITY>ERROR<MESSAGE>User or Member password invalid</STATUS><DTSERVER>20161006130327.138[-7:PDT]<LANGUAGE>ENG<FI><ORG>First Tech Federal Credit Union<FID>3169</FI></SONRS></SIGNONMSGSRSV1></OFX>";
            OFXParser parser = new OFXParser(ofx);

            // Act
            parser.Parse();

            // Assert
            Assert.IsNotNull(parser);
        }

        private string GetOfxString(OFXRequestConfigRequestType type, bool isSuccessful)
        {
            string response = "";
            string signOnFailure = "<SIGNONMSGSRSV1><SONRS><STATUS><CODE>15500<SEVERITY>ERROR<MESSAGE>User or Member password invalid</STATUS><DTSERVER>20161006130327.138[-7:PDT]<LANGUAGE>ENG<FI><ORG>First Tech Federal Credit Union<FID>3169</FI></SONRS></SIGNONMSGSRSV1>";
            string signOnSuccess = "<SIGNONMSGSRSV1><SONRS><STATUS><CODE>0<SEVERITY>INFO<MESSAGE>The operation succeeded.</STATUS><DTSERVER>20161006125941.239[-7:PDT]<LANGUAGE>ENG<FI><ORG>First Tech Federal Credit Union<FID>3169</FI></SONRS></SIGNONMSGSRSV1>";
            string accountListSuccess = "<SIGNUPMSGSRSV1><ACCTINFOTRNRS><TRNUID>1001<STATUS><CODE>0<SEVERITY>INFO</STATUS>";
            accountListSuccess += "<ACCTINFORS><DTACCTUP>20161006125454.929[-7:PDT]";
            accountListSuccess += "<ACCTINFO><DESC>Long Term Savings<BANKACCTINFO><BANKACCTFROM><BANKID>12345<BRANCHID>00<ACCTID>S12345<ACCTTYPE>SAVINGS</BANKACCTFROM><SUPTXDL>Y<XFERSRC>Y<XFERDEST>Y<SVCSTATUS>ACTIVE</BANKACCTINFO></ACCTINFO>";
            accountListSuccess += "<ACCTINFO><DESC>Carefree Checking<BANKACCTINFO><BANKACCTFROM><BANKID>12345<BRANCHID>00<ACCTID>S67890<ACCTTYPE>CHECKING</BANKACCTFROM><SUPTXDL>Y<XFERSRC>Y<XFERDEST>Y<SVCSTATUS>ACTIVE</BANKACCTINFO></ACCTINFO>";
            accountListSuccess += " </ACCTINFORS></ACCTINFOTRNRS></SIGNUPMSGSRSV1>";
            string accountListFailure = "<SIGNUPMSGSRSV1><ACCTINFOTRNRS><TRNUID>1001<STATUS><CODE>15500<SEVERITY>ERROR</STATUS></ACCTINFOTRNRS></SIGNUPMSGSRSV1>";
            string balanceSuccess = "";
            string balanceFailure = "";
            string StatementSuccess = "";
            string StatementFailure = "<BANKMSGSRSV1><STMTTRNRS><TRNUID>1001<STATUS><CODE>2003<SEVERITY>ERROR</STATUS></STMTTRNRS></BANKMSGSRSV1>";


            return response;
        }
    }
}
