using Budget.API.Services.OFXClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Budget.DAL.Models;

namespace Budget.Tests.Services.OFXClient
{
    [TestClass]
    public class OFXParserTest
    {
        [TestMethod]
        public void OFXParserSignOnSuccess()
        {
            // Arrange
            string ofx = GetOfxString(OFXRequestConfigRequestType.SignOn, true);
            OFXParser parser = new OFXParser(ofx);

            // Act
            parser.Parse();

            // Assert
            Assert.IsNotNull(parser);
            Assert.IsInstanceOfType(parser.SignOnRequest,new OFXResponseStatus().GetType());
            Assert.IsTrue(parser.SignOnRequest.Status);
            Assert.AreEqual(0, parser.SignOnRequest.Code);
            Assert.AreEqual("INFO", parser.SignOnRequest.Severity);
            Assert.IsNotNull(parser.SignOnRequest.Message);
        }

        [TestMethod]
        public void OFXParserSignOnFailure()
        {
            // Arrange
            string ofx = GetOfxString(OFXRequestConfigRequestType.SignOn, false);
            OFXParser parser = new OFXParser(ofx);

            // Act
            parser.Parse();

            // Assert
            Assert.IsNotNull(parser);
            Assert.IsInstanceOfType(parser.SignOnRequest, new OFXResponseStatus().GetType());
            Assert.IsFalse(parser.SignOnRequest.Status);
            Assert.AreNotEqual(0, parser.SignOnRequest.Code);
            Assert.AreNotEqual("INFO", parser.SignOnRequest.Severity);
            Assert.IsNotNull(parser.SignOnRequest.Message);
        }

        [TestMethod]
        public void OFXParserAccountListSuccess()
        {
            // Arrange
            string ofx = GetOfxString(OFXRequestConfigRequestType.AccountList, true);
            OFXParser parser = new OFXParser(ofx);

            // Act
            parser.Parse();

            // Assert
            Assert.IsNotNull(parser);
            Assert.IsInstanceOfType(parser.AccountListRequest, new OFXResponseStatus().GetType());
            Assert.IsTrue(parser.AccountListRequest.Status);
            Assert.AreEqual(0, parser.AccountListRequest.Code);
            Assert.AreEqual("INFO", parser.AccountListRequest.Severity);
            Assert.AreEqual(3, parser.Accounts.Count);
            Assert.AreEqual("Long Term Savings", parser.Accounts[0].Name);
            Assert.AreEqual("Long Term Savings", parser.Accounts[0].Description);
            Assert.AreEqual("S12345", parser.Accounts[0].Number);
            Assert.AreEqual(AccountType.Savings, parser.Accounts[0].Type);
            Assert.AreEqual("Carefree Checking", parser.Accounts[1].Name);
            Assert.AreEqual("Carefree Checking", parser.Accounts[1].Description);
            Assert.AreEqual("S67890", parser.Accounts[1].Number);
            Assert.AreEqual(AccountType.Checking, parser.Accounts[1].Type);
            Assert.AreEqual("CREDIT CARD", parser.Accounts[2].Name);
            Assert.AreEqual("CREDIT CARD", parser.Accounts[2].Description);
            Assert.AreEqual("1234567890", parser.Accounts[2].Number);
            Assert.AreEqual(AccountType.CreditCard, parser.Accounts[2].Type);
        }

        [TestMethod]
        public void OFXParserAccountListFailure()
        {
            // Arrange
            string ofx = GetOfxString(OFXRequestConfigRequestType.AccountList, false);
            OFXParser parser = new OFXParser(ofx);

            // Act
            parser.Parse();

            // Assert
            Assert.IsNotNull(parser);
            Assert.IsInstanceOfType(parser.AccountListRequest, new OFXResponseStatus().GetType());
            Assert.IsFalse(parser.AccountListRequest.Status);
            Assert.AreNotEqual(0, parser.AccountListRequest.Code);
            Assert.AreNotEqual("INFO", parser.AccountListRequest.Severity);
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
            accountListSuccess += "<ACCTINFO><DESC>CREDIT CARD<CCACCTINFO><CCACCTFROM><ACCTID>1234567890</CCACCTFROM><SUPTXDL>Y<XFERSRC>N<XFERDEST>N<SVCSTATUS>ACTIVE</CCACCTINFO></ACCTINFO>";
            accountListSuccess += "</ACCTINFORS></ACCTINFOTRNRS></SIGNUPMSGSRSV1>";
            string accountListFailure = "<SIGNUPMSGSRSV1><ACCTINFOTRNRS><TRNUID>1001<STATUS><CODE>15500<SEVERITY>ERROR</STATUS></ACCTINFOTRNRS></SIGNUPMSGSRSV1>";
            string balanceSuccess = "";
            string balanceFailure = "";
            string StatementSuccess = "";
            string StatementFailure = "<BANKMSGSRSV1><STMTTRNRS><TRNUID>1001<STATUS><CODE>2003<SEVERITY>ERROR</STATUS></STMTTRNRS></BANKMSGSRSV1>";
            
            // Sign On
            if (type == OFXRequestConfigRequestType.SignOn && !isSuccessful)
            {
                response += "<OFX>" + signOnFailure + "</OFX>";
                return response;
            }
            else
            {
                response += "<OFX>" + signOnSuccess;
            }

            // Account List
            if (type == OFXRequestConfigRequestType.AccountList)
            {
                if (isSuccessful)
                {
                    response += accountListSuccess;
                }
                else
                {
                    response += accountListFailure;
                }
            }

            // Balance
            if (type == OFXRequestConfigRequestType.Balance)
            {
                if (isSuccessful)
                {
                    response += balanceSuccess;
                }
                else
                {
                    response += balanceFailure;
                }
            }

            // Statement
            if (type == OFXRequestConfigRequestType.Statement)
            {
                if (isSuccessful)
                {
                    response += StatementSuccess;
                }
                else
                {
                    response += StatementFailure;
                }
            }

            response += "</OFX>";
            return response;
        }
    }
}
