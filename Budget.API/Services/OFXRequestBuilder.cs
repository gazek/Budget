using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Budget.API.Services
{

    public enum OFXRequestBuilderConfigAccountType
    {
        SAVINGS,
        CHECKING,
        CREDITCARD
    }

    public class OFXStatementRequestConfig
    {
        public string UserId { get; set; }
        public string password { get; set; }
        public string InstitutionName { get; set; }
        public int InstitutionId { get; set; }
        public int InstitutionRoutingNumber { get; set; }
        public int AccountNumber { get; set; }
        public OFXRequestBuilderConfigAccountType AccountType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeTransactions { get; set; }
        public Uri URL { get; set; }

        public OFXStatementRequestConfig()
        {
            IncludeTransactions = true;
        }

        public void VerifyConfig()
        {
            foreach (PropertyInfo prop in typeof(OFXStatementRequestConfig).GetProperties())
            {
                if (this.GetType().GetProperty(prop.Name).GetValue(this) == null)
                {
                    throw new System.ArgumentException("Config property cannot be null", prop.Name); ;
                }
            }

        }

    }

    public class OFXStatementRequestBuilder
    {
        public string Header
        { get
            {
                return this._header;
            }
        }

        public string Body
        {
            get
            {
                return this._body;
            }
        }

        public string Request
        {
            get
            {
                return this._request;
            }
        }

        public OFXStatementRequestConfig Config
        {
            get
            {
                return this._config;
            }
        }

        OFXStatementRequestConfig _config;
        string _header;
        string _body;
        string _request;
        string _APPID = "QWIN";
        string _APPVER = "1900";
        string _TRNUID = "1001";

        public OFXStatementRequestBuilder(OFXStatementRequestConfig config)
        {
            config.VerifyConfig();
            this._config = config;
            this.buildHeader();
            this.buildBody();
            this.buildRequest();
        }

        private void buildHeader ()
        {
            // set OFX request header contents
            List<List<string>> headerList = new List<List<string>>();
            headerList.Add(new List<string> { "OFXHEADER", "100" });
            headerList.Add(new List<string> { "DATA", "OFXSGML" });
            headerList.Add(new List<string> { "VERSION", "102" });
            headerList.Add(new List<string> { "SECURITY", "NONE" });
            headerList.Add(new List<string> { "ENCODING", "USASCII" });
            headerList.Add(new List<string> { "CHARSET", "1252" });
            headerList.Add(new List<string> { "COMPRESSION", "NONE" });
            headerList.Add(new List<string> { "OLDFILEUID", "NONE" });
            headerList.Add(new List<string> { "NEWFILEUID", "1" });

            // build request header string
            this._header = string.Join(" ", headerList.Select(x => string.Join(":", x)).ToArray());
        }

        string buildBodyFI()
        {
            // set OFX request body FI contents
            List<List<string>> fiList = new List<List<string>>();
            fiList.Add(new List<string> { "<FI>" });
            fiList.Add(new List<string> { "<ORG>", this._config.InstitutionName });
            fiList.Add(new List<string> { "<FID>", this._config.InstitutionId.ToString() });
            fiList.Add(new List<string> { "</FI>" });

            // build request body FI string
            return string.Join("", fiList.Select(x => string.Join("", x)).ToArray());
        }

        string buildBodySignon()
        {
            string fi = this.buildBodyFI();
            string date = DateTime.Today.ToString("yyyyMMdd");

            // set OFX request body SignOn contents
            List<List<string>> signonList = new List<List<string>>();
            signonList.Add(new List<string> { "<SIGNONMSGSRQV1>" });
            signonList.Add(new List<string> { "<SONRQ>" });
            signonList.Add(new List<string> { "<DTCLIENT>", date });
            signonList.Add(new List<string> { "<USERID>", this._config.UserId });
            signonList.Add(new List<string> { "<USERPASS>", this._config.password });
            signonList.Add(new List<string> { "<LANGUAGE>", "ENG" });
            signonList.Add(new List<string> { fi });
            signonList.Add(new List<string> { "<APPID>", this._APPID });
            signonList.Add(new List<string> { "<APPVER>", this._APPVER });
            signonList.Add(new List<string> { "</SONRQ>" });
            signonList.Add(new List<string> { "</SIGNONMSGSRQV1>" });

            // build request body SignOn string
            return string.Join("", signonList.Select(x => string.Join("", x)).ToArray());
        }

        string buildBodyAcctFrom()
        {
            if (this._config.AccountType == OFXRequestBuilderConfigAccountType.CREDITCARD)
            {
                return buildBodyAcctFromCC();
            }
            else
            {
                return buildBodyAcctFromBank();
            }
        }

        string buildBodyAcctFromBank()
        {
            // set OFX request body AcctFrom contents
            List<List<string>> AcctFromList = new List<List<string>>();
            AcctFromList.Add(new List<string> { "<BANKACCTFROM>" });
            AcctFromList.Add(new List<string> { "<BANKID>", this._config.InstitutionRoutingNumber.ToString() });
            AcctFromList.Add(new List<string> { "<ACCTID>", this._config.AccountNumber.ToString() });
            AcctFromList.Add(new List<string> { "<ACCTTYPE>", this._config.AccountType.ToString() });
            AcctFromList.Add(new List<string> { "</BANKACCTFROM>" });

            // build request body AcctFrom string
            return string.Join("", AcctFromList.Select(x => string.Join("", x)).ToArray());
        }

        string buildBodyAcctFromCC()
        {
            // set OFX request body AcctFrom contents
            List<List<string>> AcctFromList = new List<List<string>>();
            AcctFromList.Add(new List<string> { "<CCACCTFROM>" });
            AcctFromList.Add(new List<string> { "<ACCTID>", this._config.AccountNumber.ToString() });
            AcctFromList.Add(new List<string> { "</CCACCTFROM>" });

            // build request body AcctFrom string
            return string.Join("", AcctFromList.Select(x => string.Join("", x)).ToArray());
        }

        string buildBodyIncTran()
        {
            string include = "Y";
            if (!this._config.IncludeTransactions)
            {
                include = "N";
            }

            // set OFX request body IncTran contents
            List<List<string>> IncTranList = new List<List<string>>();
            IncTranList.Add(new List<string> { "<INCTRAN>" });
            IncTranList.Add(new List<string> { "<DTSTART>", this._config.StartDate.ToString("yyyyMMdd") });
            IncTranList.Add(new List<string> { "<DTEND>", this._config.EndDate.ToString("yyyyMMdd") });
            IncTranList.Add(new List<string> { "<INCLUDE>" , include });
            IncTranList.Add(new List<string> { "</INCTRAN>" });

            // build request body IncTran string
            return string.Join("", IncTranList.Select(x => string.Join("", x)).ToArray());
        }

        string buildBodyMsgSRq()
        {
            string ACCTFROM = buildBodyAcctFrom();
            string INCTRAN = buildBodyIncTran();
            string MSGSRQ = "BANKMSGSRQV1>";
            string STMTTRNRQ = "STMTTRNRQ>";
            string STMTRQ = "STMTRQ>";
            if (this._config.AccountType == OFXRequestBuilderConfigAccountType.CREDITCARD)
            {
                MSGSRQ = "CREDITCARDMSGSRQV1>";
                STMTTRNRQ = "CCSTMTTRNRQ>";
                STMTRQ = "CCSTMTRQ>";
            }

            // set OFX request body Message Set Request contents
            List<List<string>> MsgSRqList = new List<List<string>>();
            MsgSRqList.Add(new List<string> { "<" + MSGSRQ });
            MsgSRqList.Add(new List<string> { "<" + STMTTRNRQ });
            MsgSRqList.Add(new List<string> { "<TRNUID>", this._TRNUID });
            MsgSRqList.Add(new List<string> { "<" + STMTRQ });
            MsgSRqList.Add(new List<string> { ACCTFROM });
            MsgSRqList.Add(new List<string> { INCTRAN });
            MsgSRqList.Add(new List<string> { "</" + STMTRQ });
            MsgSRqList.Add(new List<string> { "</" + STMTTRNRQ });
            MsgSRqList.Add(new List<string> { "</" + MSGSRQ });

            // build request body Message Set Request string
            return string.Join("", MsgSRqList.Select(x => string.Join("", x)).ToArray());
        }

        void buildBody()
        {
            string signon = buildBodySignon();
            string msgSRq = buildBodyMsgSRq();

            // set OFX request body contents
            List<List<string>> bodyList = new List<List<string>>();
            bodyList.Add(new List<string> { "<OFX>" });
            bodyList.Add(new List<string> { signon });
            bodyList.Add(new List<string> { msgSRq });
            bodyList.Add(new List<string> { "</OFX>" });

            // build request body Message Set Request string
            this._body = string.Join("", bodyList.Select(x => string.Join("", x)).ToArray());
        }

        private void buildRequest()
        {
            this._request = this._header + "\n\n" + this._body;
        }

    }
}