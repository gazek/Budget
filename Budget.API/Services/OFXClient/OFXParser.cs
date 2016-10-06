using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Budget.API.Services.OFXClient
{
    public class OFXResponseStatus
    {
        public bool Status { get; set; }
        public int Code { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
    }

    public class OFXParser
    {
        // SignOn
        public OFXResponseStatus SignOnRequest { get { return _signOnRequest; } }
        private OFXResponseStatus _signOnRequest;

        // Account List
        public OFXResponseStatus AccountListRequest { get { return _accountListRequest; } }
        private OFXResponseStatus _accountListRequest;

        // Balance
        public OFXResponseStatus BalanceRequest { get { return _balanceRequest; } }
        private OFXResponseStatus _balanceRequest;

        // Statment
        public OFXResponseStatus StatmentRequest { get { return _statmentRequest; } }
        private OFXResponseStatus _statmentRequest;

        // internal OFX and XML Doc
        string _ofx;
        XmlDocument _doc;

        public OFXParser(string ofx)
        {
            _ofx = OFXTagCloser.CloseTags(ofx);
            _signOnRequest = new OFXResponseStatus();
            _accountListRequest = new OFXResponseStatus();
            _balanceRequest = new OFXResponseStatus();
            _statmentRequest = new OFXResponseStatus();
        }

        public void Parse()
        {
            _doc = new XmlDocument();
            _doc.PreserveWhitespace = true;
            _doc.LoadXml(_ofx);

            // parse signon info
            ParseSignOn();

            // Continue if signon was successfull
            if (_signOnRequest.Status)
            {
                // look for account list response to parse
                ParseAccountList();
                // look for statement response to parse
                ParseStatement();
                // look for statement response to parse
                ParseBalance();
            }
        }

        private void ParseSignOn()
        {
            // set base location to relevent signon info
            XmlNode doc = _doc.SelectSingleNode("OFX/SIGNONMSGSRSV1/SONRS/STATUS");

            // make sure there is a signon block
            if (doc == null)
            {
                _signOnRequest.Status = false;
                return;
            }

            // parse signon info
            ParseStatus(doc, _signOnRequest);
        }

        private void ParseAccountList()
        {
            // set base location to account block
            string basePath = "OFX/SIGNUPMSGSRSV1/ACCTINFOTRNRS/";

            // parse status
            XmlNode status = _doc.SelectSingleNode(basePath+"STATUS");
            ParseStatus(status, _accountListRequest);

            // parse account list
            if (_accountListRequest.Status)
            {

            }
        }

        private void ParseBalance()
        {
            // set base location to account block
            string basePath = "OFX/BANKMSGSRSV1/STMTTRNRS";

            // parse status
            XmlNode status = _doc.SelectSingleNode(basePath + "STATUS");
            ParseStatus(status, _balanceRequest);

            // parse balance
            if (_accountListRequest.Status)
            {

            }
        }

        private void ParseStatement()
        {
            // set base location to account block
            string basePath = "OFX/BANKMSGSRSV1/STMTTRNRS";

            // parse status
            XmlNode status = _doc.SelectSingleNode(basePath + "STATUS");
            ParseStatus(status, _statmentRequest);

            // parse statment
            if (_accountListRequest.Status)
            {

            }
        }

        private void ParseStatus(XmlNode doc, OFXResponseStatus status)
        {
            // grab CODE tag
            string rawStatusCode = doc.SelectSingleNode("CODE").InnerText;

            // try to convert to int
            int code;
            if (!Int32.TryParse(rawStatusCode, out code))
            {
                status.Status = false;
                status.Code = -1;
            }
            status.Code = code;

            // set severity
            status.Severity = doc.SelectSingleNode("SEVERITY").InnerText;

            // set message if present
            XmlNode message = doc.SelectSingleNode("MESSAGE");
            if (message != null)
            {
                status.Message = message.InnerText;
            }  
        }
    }

}