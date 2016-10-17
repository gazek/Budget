using System;
using System.Collections.Generic;
using System.Linq;

namespace Budget.API.Services.OFXClient
{
    public class OFXRequestBuilder
    {
        public string Header { get { return _header; } }
        string _header;

        public string Body { get { return _body; } }
        string _body;

        public string Request { get { return _request; } }
        string _request;

        public OFXRequestConfig Config { get { return _config; } }
        OFXRequestConfig _config;
        
        // TODO: move this to a config
        string _APPID = "QWIN";
        string _APPVER = "2200";
        string _TRNUID = "1001";
        string _DTACCTUP = "20000101";
        string _VERSION = "103";
        string _LANGUAGE = "ENG";
        string _DATA = "OFXSGML";
        string _OFXHEADER = "100";
        string _SECURITY = "NONE";
        string _ENCODING = "USASCII";
        string _CHARSET = "1252";
        string _COMPRESSION = "NONE";
        string _OLDFILEUID = "NONE";
        string _NEWFILEUID = "NONE";

        public OFXRequestBuilder(OFXRequestConfig config)
        {
            config.VerifyConfig();
            _config = config;
            buildHeader();
            buildBody();
            buildRequest();
        }

        private void buildHeader ()
        {
            // set OFX request header contents
            List<List<string>> headerList = new List<List<string>>();
            headerList.Add(new List<string> { "OFXHEADER", _OFXHEADER });
            headerList.Add(new List<string> { "DATA", _DATA });
            headerList.Add(new List<string> { "VERSION", _VERSION });
            headerList.Add(new List<string> { "SECURITY", _SECURITY });
            headerList.Add(new List<string> { "ENCODING", _ENCODING });
            headerList.Add(new List<string> { "CHARSET", _CHARSET });
            headerList.Add(new List<string> { "COMPRESSION", _COMPRESSION });
            headerList.Add(new List<string> { "OLDFILEUID", _OLDFILEUID });
            headerList.Add(new List<string> { "NEWFILEUID", _NEWFILEUID });

            // build request header string
            _header = string.Join(" ", headerList.Select(x => string.Join(":", x)).ToArray());
        }

        string buildBodyFI()
        {
            // set OFX request body FI contents
            List<List<string>> fiList = new List<List<string>>();
            fiList.Add(new List<string> { "<FI>" });
            fiList.Add(new List<string> { "<ORG>", _config.InstitutionName });
            fiList.Add(new List<string> { "<FID>", _config.InstitutionId.ToString() });
            fiList.Add(new List<string> { "</FI>" });

            // build request body FI string
            return string.Join("", fiList.Select(x => string.Join("", x)).ToArray());
        }

        string buildBodySignon()
        {
            string fi = buildBodyFI();
            string date = DateTime.Today.ToString("yyyyMMdd");

            // set OFX request body SignOn contents
            List<List<string>> signonList = new List<List<string>>();
            signonList.Add(new List<string> { "<SIGNONMSGSRQV1>" });
            signonList.Add(new List<string> { "<SONRQ>" });
            signonList.Add(new List<string> { "<DTCLIENT>", date });
            signonList.Add(new List<string> { "<USERID>", _config.UserId });
            signonList.Add(new List<string> { "<USERPASS>", _config.password });
            signonList.Add(new List<string> { "<LANGUAGE>", _LANGUAGE });
            signonList.Add(new List<string> { fi });
            signonList.Add(new List<string> { "<APPID>", _APPID });
            signonList.Add(new List<string> { "<APPVER>", _APPVER });
            signonList.Add(new List<string> { "</SONRQ>" });
            signonList.Add(new List<string> { "</SIGNONMSGSRQV1>" });

            // build request body SignOn string
            return string.Join("", signonList.Select(x => string.Join("", x)).ToArray());
        }

        string buildBodyAcctFrom()
        {
            if (_config.AccountType == OFXRequestConfigAccountType.CREDITCARD)
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
            AcctFromList.Add(new List<string> { "<BANKID>", _config.InstitutionRoutingNumber.ToString() });
            AcctFromList.Add(new List<string> { "<ACCTID>", _config.AccountNumber });
            AcctFromList.Add(new List<string> { "<ACCTTYPE>", _config.AccountType.ToString() });
            AcctFromList.Add(new List<string> { "</BANKACCTFROM>" });

            // build request body AcctFrom string
            return string.Join("", AcctFromList.Select(x => string.Join("", x)).ToArray());
        }

        string buildBodyAcctFromCC()
        {
            // set OFX request body AcctFrom contents
            List<List<string>> AcctFromList = new List<List<string>>();
            AcctFromList.Add(new List<string> { "<CCACCTFROM>" });
            AcctFromList.Add(new List<string> { "<ACCTID>", _config.AccountNumber });
            AcctFromList.Add(new List<string> { "</CCACCTFROM>" });

            // build request body AcctFrom string
            return string.Join("", AcctFromList.Select(x => string.Join("", x)).ToArray());
        }

        string buildBodyIncTran()
        {
            string include = "N";
            if (_config.RequestType == OFXRequestConfigRequestType.Statement)
            {
                include = "Y";
            }

            // set OFX request body IncTran contents
            List<List<string>> IncTranList = new List<List<string>>();
            IncTranList.Add(new List<string> { "<INCTRAN>" });
            IncTranList.Add(new List<string> { "<DTSTART>", _config.StartDate.ToString("yyyyMMdd") });
            IncTranList.Add(new List<string> { "<DTEND>", _config.EndDate.ToString("yyyyMMdd") });
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
            if (_config.AccountType == OFXRequestConfigAccountType.CREDITCARD)
            {
                MSGSRQ = "CREDITCARDMSGSRQV1>";
                STMTTRNRQ = "CCSTMTTRNRQ>";
                STMTRQ = "CCSTMTRQ>";
            }

            // set OFX request body Message Set Request contents
            List<List<string>> MsgSRqList = new List<List<string>>();
            MsgSRqList.Add(new List<string> { "<" + MSGSRQ });
            MsgSRqList.Add(new List<string> { "<" + STMTTRNRQ });
            MsgSRqList.Add(new List<string> { "<TRNUID>", _TRNUID });
            MsgSRqList.Add(new List<string> { "<" + STMTRQ });
            MsgSRqList.Add(new List<string> { ACCTFROM });
            MsgSRqList.Add(new List<string> { INCTRAN });
            MsgSRqList.Add(new List<string> { "</" + STMTRQ });
            MsgSRqList.Add(new List<string> { "</" + STMTTRNRQ });
            MsgSRqList.Add(new List<string> { "</" + MSGSRQ });

            // build request body Message Set Request string
            return string.Join("", MsgSRqList.Select(x => string.Join("", x)).ToArray());
        }

        string buildSignUpMsgSRqAccountList()
        {
            // set OFX request body Message Set Request contents
            List<List<string>> MsgSRqList = new List<List<string>>();
            MsgSRqList.Add(new List<string> { "<SIGNUPMSGSRQV1>" });
            MsgSRqList.Add(new List<string> { "<ACCTINFOTRNRQ>" });
            MsgSRqList.Add(new List<string> { "<TRNUID>", _TRNUID });
            MsgSRqList.Add(new List<string> { "<ACCTINFORQ>" });
            MsgSRqList.Add(new List<string> { "<DTACCTUP>", _DTACCTUP });
            MsgSRqList.Add(new List<string> { "</ACCTINFORQ>" });
            MsgSRqList.Add(new List<string> { "</ACCTINFOTRNRQ>" });
            MsgSRqList.Add(new List<string> { "</SIGNUPMSGSRQV1>" });

            // build request body Message Set Request string
            return string.Join("", MsgSRqList.Select(x => string.Join("", x)).ToArray());
        }

        void buildBody()
        {
            string signon = buildBodySignon();
            string msgSRq = "";
            switch (_config.RequestType)
            {
                case OFXRequestConfigRequestType.Statement:
                    msgSRq = buildBodyMsgSRq();
                    break;
                case OFXRequestConfigRequestType.Balance:
                    msgSRq = buildBodyMsgSRq();
                    break;
                case OFXRequestConfigRequestType.AccountList:
                    msgSRq = buildSignUpMsgSRqAccountList();
                    break;
                case OFXRequestConfigRequestType.SignOn:
                    msgSRq = "";
                    break;
            }

            // set OFX request body contents
            List<List<string>> bodyList = new List<List<string>>();
            bodyList.Add(new List<string> { "<OFX>" });
            bodyList.Add(new List<string> { signon });
            bodyList.Add(new List<string> { msgSRq });
            bodyList.Add(new List<string> { "</OFX>" });

            // build request body Message Set Request string
            _body = string.Join("", bodyList.Select(x => string.Join("", x)).ToArray());
        }

        private void buildRequest()
        {
            _request = _header + "\n\n" + _body;
        }

    }
}