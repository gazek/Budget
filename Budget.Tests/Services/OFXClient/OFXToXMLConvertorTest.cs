using Budget.API.Services.OFXClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Budget.Tests.Services.OFXClient
{
    [TestClass]
    public class OFXToXMLConvertorTest
    {
        [TestMethod]
        public void OFXCloseTagsTest()
        {
            // Arrange
            OFXToXMLConvertor convertor = new OFXToXMLConvertor();
            string expectedXml = "<OFX><SIGNONMSGSRSV1><SONRS><STATUS><CODE>15500</CODE><SEVERITY>ERROR</SEVERITY><MESSAGE>User or Member password invalid</MESSAGE></STATUS><DTSERVER>20161005123252.423[-7:PDT]</DTSERVER><LANGUAGE>ENG</LANGUAGE><FI><ORG>First Tech Federal Credit Union</ORG><FID>3169</FID></FI></SONRS></SIGNONMSGSRSV1></OFX>";
            convertor.Ofx = "<OFX><SIGNONMSGSRSV1><SONRS><STATUS><CODE>15500<SEVERITY>ERROR<MESSAGE>User or Member password invalid</STATUS><DTSERVER>20161005123252.423[-7:PDT]<LANGUAGE>ENG<FI><ORG>First Tech Federal Credit Union<FID>3169</FI></SONRS></SIGNONMSGSRSV1></OFX>";

            // Act
            convertor.Convert();

            // Assert
            Assert.AreEqual(convertor.Xml, expectedXml);
        }
    }
}
