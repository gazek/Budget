using Budget.API.Services;
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
                "VERSION:102",
                "SECURITY:NONE",
                "ENCODING:USASCII",
                "CHARSET:1252",
                "COMPRESSION:NONE",
                "OLDFILEUID:NONE",
                "NEWFILEUID:1" };
            string expectedHeader = string.Join(" ", header);
            var config = CreateValidRequestBuilderConfig();
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

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
            var config = new OFXRequestBuilderConfig();

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert

        }

        [TestMethod]
        public void OFXRequestBuilderBodyBankTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            string expectedBody = GetValidRequestBodyString(OFXRequestBuilderConfigAccountType.CHECKING, true);

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
        }

        [TestMethod]
        public void OFXRequestBuilderBodyCreditCardTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.AccountType = OFXRequestBuilderConfigAccountType.CREDITCARD;
            string expectedBody = GetValidRequestBodyString(OFXRequestBuilderConfigAccountType.CREDITCARD, true);

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
        }

        [TestMethod]
        public void OFXRequestBuilderBodyIncludeTransTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
            config.IncludeTransactions = false;
            string expectedBody = GetValidRequestBodyString(OFXRequestBuilderConfigAccountType.CHECKING, false);

            // Act
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
        }

        private OFXRequestBuilderConfig CreateValidRequestBuilderConfig()
        {
            var config = new OFXRequestBuilderConfig()
            {
                UserId = "testUser",
                password = "testPassword",
                InstitutionName = "Some Bank",
                InstitutionId = 123,
                InstitutionRoutingNumber = 456,
                AccountNumber = 789,
                AccountType = OFXRequestBuilderConfigAccountType.CHECKING,
                StartDate = new DateTime(2016, 9, 1),
                EndDate = new DateTime(2016, 9, 30),
                //IncludeTransactions = true
            };
            return config;
        }

        private string GetValidRequestBodyString(OFXRequestBuilderConfigAccountType type, bool includeTrans)
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

            if (type == OFXRequestBuilderConfigAccountType.CREDITCARD)
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
                "<OFX>",
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
                "</SIGNONMSGSRQV1>",
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
                BANKMSGSRQV1CLOSE,
                "</OFX>" };

            string result = string.Join("", body);
            return result;
        }
    }
}
