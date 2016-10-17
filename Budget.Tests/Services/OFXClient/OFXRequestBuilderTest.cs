using Budget.API.Services.OFXClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Budget.Tests.Controllers
{
    [TestClass]
    public class OFXRequestBuilderTest
    {

        [TestMethod]
        public void OFXRequestBuilderConstructorTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.IsNotNull(ofxBuilder);
        }

        [TestMethod]
        public void OFXRequestBuilderHeaderTest()
        {
            // Arrange
            string[] header = {
                "OFXHEADER:100",
                "DATA:OFXSGML",
                "VERSION:103",
                "SECURITY:NONE",
                "ENCODING:USASCII",
                "CHARSET:1252",
                "COMPRESSION:NONE",
                "OLDFILEUID:NONE",
                "NEWFILEUID:NONE" };
            string expectedHeader = string.Join(" ", header);
            var config = CreateValidRequestBuilderConfig();
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Act
            string result = ofxBuilder.Header;

            //Assert
            Assert.AreEqual(expectedHeader, result);
        }

        [TestMethod]
        public void OFXRequestBuildeBankStatementWithoutCLientUIDTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            string expectedBody = GetValidStatementRequestBodyString(OFXRequestConfigAccountType.CHECKING, false);

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(expectedBody, ofxBuilder.Body);
        }

        [TestMethod]
        public void OFXRequestBuildeBankStatementWithCLientUIDTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig(true);
            string expectedBody = GetValidStatementRequestBodyString(OFXRequestConfigAccountType.CHECKING, true);

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(expectedBody, ofxBuilder.Body);
        }

        [TestMethod]
        public void OFXRequestBuilderCreditCardStatementWithoutCLientUIDTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.AccountType = OFXRequestConfigAccountType.CREDITCARD;
            string expectedBody = GetValidStatementRequestBodyString(OFXRequestConfigAccountType.CREDITCARD, false);

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(expectedBody, ofxBuilder.Body);
        }

        [TestMethod]
        public void OFXRequestBuilderBalanceWithoutCLientUIDTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.RequestType = OFXRequestConfigRequestType.Balance;
            string expectedBody = GetValidBalanceRequestBodyString(OFXRequestConfigAccountType.CHECKING, false);

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(expectedBody, ofxBuilder.Body);
        }

        [TestMethod]
        public void OFXRequestBuilderAccountListWithoutCLientUIDTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.RequestType = OFXRequestConfigRequestType.AccountList;
            string expectedBody = GetValidAccountListRequestBodyString(false);

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(expectedBody, ofxBuilder.Body);
        }

        [TestMethod]
        public void OFXRequestBuilderSignOnWithoutCLientUIDTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.RequestType = OFXRequestConfigRequestType.SignOn;
            string expectedBody = GetValidSignOnRequestBodyString(false);

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(expectedBody, ofxBuilder.Body);
        }

        private OFXRequestConfig CreateValidRequestBuilderConfig(bool includeClientUID = false)
        {
            var config = new OFXRequestConfig()
            {
                RequestType = OFXRequestConfigRequestType.Statement,
                UserId = "testUser",
                password = "testPassword",
                InstitutionName = "Some Bank",
                InstitutionId = 123,
                InstitutionRoutingNumber = 456,
                AccountNumber = "789",
                AccountType = OFXRequestConfigAccountType.CHECKING,
                StartDate = new DateTime(2016, 9, 1),
                EndDate = new DateTime(2016, 9, 30),
                URL = new Uri("https://fake.com")
            };

            if (includeClientUID)
            {
                config.ClientUID = new Guid("09fd60be-ecb0-447e-ab4a-4c433b853a10");
            }

            return config;
        }

        private string GetValidSignOnRequestString(bool includeClientUID=false)
        {
            string clientUID="";
            if (includeClientUID)
            {
                clientUID = "<CLIENTUID>09fd60be-ecb0-447e-ab4a-4c433b853a10";
            }
            string[] body = {
                "<SIGNONMSGSRQV1>",
                "<SONRQ>",
                "<DTCLIENT>"+DateTime.Today.ToString("yyyyMMdd"),
                "<USERID>testUser",
                "<USERPASS>testPassword",
                "<LANGUAGE>ENG",
                "<FI>",
                "<ORG>Some Bank",
                "<FID>123",
                "</FI>",
                "<APPID>QWIN",
                "<APPVER>2200",
                clientUID,
                "</SONRQ>",
                "</SIGNONMSGSRQV1>"
            };

            string result = string.Join("", body);
            return result;
        }

        private string GetValidAccountListRequestString()
        {
            string[] body = {
                "<SIGNUPMSGSRQV1>",
                "<ACCTINFOTRNRQ>",
                "<TRNUID>1001",
                "<ACCTINFORQ>",
                "<DTACCTUP>20000101",
                "</ACCTINFORQ>",
                "</ACCTINFOTRNRQ>",
                "</SIGNUPMSGSRQV1>"
            };

            string result = string.Join("", body);
            return result;
        }

        private string GetValidBankRequestString(OFXRequestConfigAccountType type, bool includeTrans)
        {
            string BANKMSGSRQV1 = "<BANKMSGSRQV1>";
            string STMTTRNRQ = "<STMTTRNRQ>";
            string STMTRQ = "<STMTRQ>";
            string ACCTFROM = "<BANKACCTFROM>";
            string BANKID = "<BANKID>456";
            string ACCTTYPE = "<ACCTTYPE>"+type.ToString();
            string ACCTFROMCLOSE = "</BANKACCTFROM>";
            string STMTRQCLOSE = "</STMTRQ>";
            string STMTTRNRQCLOSE = "</STMTTRNRQ>";
            string BANKMSGSRQV1CLOSE = "</BANKMSGSRQV1>";
            string include = "Y";

            if (type == OFXRequestConfigAccountType.CREDITCARD)
            {
                BANKMSGSRQV1 = "<CREDITCARDMSGSRQV1>";
                STMTTRNRQ = "<CCSTMTTRNRQ>";
                STMTRQ = "<CCSTMTRQ>";
                ACCTFROM = "<CCACCTFROM>";
                BANKID = "";
                ACCTTYPE = "";
                ACCTFROMCLOSE = "</CCACCTFROM>";
                STMTRQCLOSE = "</CCSTMTRQ>";
                STMTTRNRQCLOSE = "</CCSTMTTRNRQ>";
                BANKMSGSRQV1CLOSE = "</CREDITCARDMSGSRQV1>";
            }

            if (!includeTrans)
            {
                include = "N";
            }

            string[] body = {
                BANKMSGSRQV1,
                STMTTRNRQ,
                "<TRNUID>1001",
                STMTRQ,
                ACCTFROM,
                BANKID,
                "<ACCTID>789",
                ACCTTYPE,
                ACCTFROMCLOSE,
                "<INCTRAN>",
                "<DTSTART>20160901",
                "<DTEND>20160930",
                "<INCLUDE>"+include,
                "</INCTRAN>",
                STMTRQCLOSE,
                STMTTRNRQCLOSE,
                BANKMSGSRQV1CLOSE
            };

            string result = string.Join("", body);
            return result;
        }

        string GetValidStatementRequestBodyString(OFXRequestConfigAccountType accountType, bool includeClientUID=false)
        {
            string signon = GetValidSignOnRequestString(includeClientUID);
            string request = GetValidBankRequestString(accountType, true);
            return "<OFX>" + signon + request + "</OFX>";
        }

        string GetValidBalanceRequestBodyString(OFXRequestConfigAccountType accountType, bool includeClientUID = false)
        {
            string signon = GetValidSignOnRequestString(includeClientUID);
            string request = GetValidBankRequestString(accountType, false);
            return "<OFX>" + signon + request + "</OFX>";
        }

        string GetValidAccountListRequestBodyString(bool includeClientUID = false)
        {
            string signon = GetValidSignOnRequestString(includeClientUID);
            string request = GetValidAccountListRequestString();
            return "<OFX>" + signon + request + "</OFX>";
        }

        string GetValidSignOnRequestBodyString(bool includeClientUID = false)
        {
            string signon = GetValidSignOnRequestString(includeClientUID);
            return "<OFX>" + signon + "</OFX>";
        }
    }
}
