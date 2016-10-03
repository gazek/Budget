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
            OFXRequestBuilder ofxBuilder = new OFXRequestBuilder(config);

            // Assert
            Assert.AreEqual(ofxBuilder.Body, expectedBody);
            //string result = GetOfxField(ofxBuilder.Body, "ORG");
            //Assert.AreEqual(result, "Some Bank");
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

        private string GetOfxField(string ofx, string field)
        {
            XmlDocument doc = OfxToXml(ofx);
            var result = doc.GetElementsByTagName(field);
            
            return "foo";
        }

        XmlDocument OfxToXml(string ofx)
        {

            // setup SgmlReader
            Sgml.SgmlReader sgmlReader = new Sgml.SgmlReader();
            //sgmlReader.DocType = "OFX";
            sgmlReader.WhitespaceHandling = WhitespaceHandling.All;
            //sgmlReader.CaseFolding = Sgml.CaseFolding.ToLower;

            // create document
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.XmlResolver = null;

            using (TextReader sr = new StringReader(ofx))
            {
                sgmlReader.InputStream = sr;
                doc.Load(sgmlReader);
            }
            
            return doc;
        }
        
    }
}
