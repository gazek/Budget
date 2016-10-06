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
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

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
                "VERSION:102",
                "SECURITY:NONE",
                "ENCODING:USASCII",
                "CHARSET:1252",
                "COMPRESSION:NONE",
                "OLDFILEUID:NONE",
                "NEWFILEUID:NONE" };
            string expectedHeader = string.Join(" ", header);
            var config = CreateValidRequestBuilderConfig();
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

            // Act
            string result = ofxBuilder.Header;

            //Assert
            Assert.AreEqual(result, expectedHeader);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Config property cannot be null")]
        public void OFXRequestBuilderConfigTest()
        {
            // Arrange
            var config = new OFXRequestConfig();

            // Act
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

            // Assert

        }

        [TestMethod]
        public void OFXRequestBuildeBankStatementTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            string expectedBody = GetValidStatementRequestBodyString(OFXRequestConfigAccountType.CHECKING);

            // Act
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
        }

        [TestMethod]
        public void OFXRequestBuilderCreditCardStatementTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.AccountType = OFXRequestConfigAccountType.CREDITCARD;
            string expectedBody = GetValidStatementRequestBodyString(OFXRequestConfigAccountType.CREDITCARD);

            // Act
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
        }

        [TestMethod]
        public void OFXRequestBuilderBalanceTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.RequestType = OFXRequestConfigRequestType.Balance;
            string expectedBody = GetValidBalanceRequestBodyString(OFXRequestConfigAccountType.CHECKING);

            // Act
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
        }

        [TestMethod]
        public void OFXRequestBuilderAccountListTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.RequestType = OFXRequestConfigRequestType.AccountList;
            string expectedBody = GetValidAccountListRequestBodyString();

            // Act
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
        }

        [TestMethod]
        public void OFXRequestBuilderSignOnTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.RequestType = OFXRequestConfigRequestType.SignOn;
            string expectedBody = GetValidSignOnRequestBodyString();

            // Act
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
        }

        private OFXRequestConfig CreateValidRequestBuilderConfig()
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
            return config;
        }

        private string GetValidSignOnRequestString()
        {
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
                "<APPVER>1900",
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

        string GetValidStatementRequestBodyString(OFXRequestConfigAccountType accountType)
        {
            string signon = GetValidSignOnRequestString();
            string request = GetValidBankRequestString(accountType, true);
            return "<OFX>" + signon + request + "</OFX>";
        }

        string GetValidBalanceRequestBodyString(OFXRequestConfigAccountType accountType)
        {
            string signon = GetValidSignOnRequestString();
            string request = GetValidBankRequestString(accountType, false);
            return "<OFX>" + signon + request + "</OFX>";
        }

        string GetValidAccountListRequestBodyString()
        {
            string signon = GetValidSignOnRequestString();
            string request = GetValidAccountListRequestString();
            return "<OFX>" + signon + request + "</OFX>";
        }

        string GetValidSignOnRequestBodyString()
        {
            string signon = GetValidSignOnRequestString();
            return "<OFX>" + signon + "</OFX>";
        }
    }
}
