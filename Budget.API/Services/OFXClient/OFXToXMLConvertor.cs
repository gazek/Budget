using System.Text;

namespace Budget.API.Services.OFXClient
{
    public class OFXToXMLConvertor
    {
        // Converting from (pre-version 2.0) OFX to XML is just a matter of
        // adding closing tags to atom elements

        public string Ofx
        {
            get { return _ofx; }
            set { _ofx = value.Replace("\r\n", string.Empty); }
        }
        public string Xml
        {
            get { return _xml; }
        }
        string _xml;
        string _ofx;

        public void Convert()
        {
            // build new xml string
            StringBuilder xml = new StringBuilder();

            // initialize to first char of input OFX string
            xml.Append(_ofx[0]);

            // last tag scanned
            string tag = "";

            // track when the index is inside a tag
            bool inTag = _ofx[0] == '<' ? true : false;

            // track previous char
            char prevChar = _ofx[0];

            // walk across the string and 
            // and place a closing tag whenever a '<' is found
            // that wasn't preceded by a '>'
            for (int i = 1; i < _ofx.Length; ++i)
            {
                // Clear previous tag name when new tag openning found
                if (prevChar == '<')
                {
                    tag = "";
                    inTag = true;
                }

                // Add closing tag
                if (_ofx[i] == '<' && prevChar != '>')
                {
                    xml.Append("</"+tag+">");
                }

                // build XML string
                xml.Append(_ofx[i]);

                // build up tag name
                if (inTag)
                {
                    if (_ofx[i] == '>')
                    {
                        inTag = false;
                    }
                    else
                    {
                        tag += _ofx[i].ToString();
                    }
                }

                // set prev char
                prevChar = _ofx[i];
            }
            _xml = xml.ToString();
        }
    }
}