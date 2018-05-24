using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Budget.API.Services.OFXClient
{
    public static class OFXUtils
    {
        public static Dictionary<string, string> PartitionResponse(string ofx)
        {
            string s = Regex.Replace(ofx, @"\t|\n|\r| ", "");
            int ofxStartIndex = s.IndexOf("<OFX>");
            int ofxEndIndex = s.IndexOf("</OFX>") + "</OFX>".Length;

            string header = ofxStartIndex >= 0 ? s.Substring(0, ofxStartIndex) : null;
            string body = ofxStartIndex >= 0 && ofxEndIndex > ofxStartIndex ? s.Substring(ofxStartIndex, ofxEndIndex - ofxStartIndex) : null;

            var result = new Dictionary<string, string>
            {
                { "header", header },
                { "body", body }
            };
            return result;
        }
    }
}