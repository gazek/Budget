using Budget.API.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Xml;

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
                "NEWFILEUID:1" };
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
            var config = new OFXStatementRequestConfig();

            // Act
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

            // Assert

        }

        [TestMethod]
        public void OFXRequestBuilderBodyTest()
        {
            // Arrange
            var config = CreateValidRequestBuilderConfig();
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
                "<BANKMSGSRQV1>",
                "<STMTTRNRQ>",
                "<TRNUID>1001",
                "<STMTRQ>",
                "<BANKACCTFROM>",
                "<BANKID>456",
                "<ACCTID>789",
                "<ACCTTYPE>CHECKING",
                "</BANKACCTFROM>",
                "<INCTRAN>",
                "<DTSTART>20160901",
                "<DTEND>20160930",
                "<INCLUDE>Y",
                "</INCTRAN>",
                "</STMTRQ>",
                "</STMTTRNRQ>",
                "</BANKMSGSRQV1>",
                "</OFX>" };
            string expectedBody = string.Join("", body);


            // Act
            OFXStatementRequestBuilder ofxBuilder = new OFXStatementRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
        }

        private OFXStatementRequestConfig CreateValidRequestBuilderConfig()
        {
            var config = new OFXStatementRequestConfig()
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
                URL = new Uri("https://fake.com"),
                //IncludeTransactions = true
            };
            return config;
        }
    }
}
