﻿using System;
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
        string _ofxType;

        // paths to XML nodes
        Dictionary<string, string> _ofxPath = new Dictionary<string, string>()
        {
            { "signOnStatus", "OFX/SIGNONMSGSRSV1/SONRS/STATUS" },
            { "accountListStatus", "OFX/SIGNUPMSGSRSV1/ACCTINFOTRNRS/STATUS" },
            { "accountListData", "OFX/SIGNUPMSGSRSV1/ACCTINFOTRNRS" },
            { "bankBalanceStatus", "OFX/BANKMSGSRSV1/STMTTRNRS/STATUS"},
            { "bankBalanceData", "OFX/SIGNUPMSGSRSV1/ACCTINFOTRNRS" },
            { "ccBalanceStatus", "OFX/CREDITCARDMSGSRSV1/CCSTMTTRNRS/STATUS" },
            { "ccBalanceData", "OFX/CREDITCARDMSGSRSV1/CCSTMTTRNRS/CCSTMTRS/LEDGERBAL" },
            { "bankStatementStatus", "OFX/BANKMSGSRSV1/STMTTRNRS/STATUS" },
            { "bankStatementData", "OFX/CREDITCARDMSGSRSV1/CCSTMTTRNRS/CCSTMTRS/LEDGERBAL" },
            { "ccStatementStatus", "OFX/CREDITCARDMSGSRSV1/CCSTMTTRNRS/STATUS" },
            { "ccStatementData", "OFX/CREDITCARDMSGSRSV1/CCSTMTTRNRS/CCSTMTRS/BANKTRANLIST" },
            { "balanceStatus", "" },
            { "balanceData", "" },
            { "statementStatus", "" },
            { "statementData", "" }
        };
        
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

            // determine request type
            GetStatementType();

            // set node paths
            SetStatementNodePaths();

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

        private void GetStatementType()
        {
            if (_doc.GetElementsByTagName("CREDITCARDMSGSRSV1").Count > 0)
            {
                _ofxType = "cc";
                return;
            }

            if (_doc.GetElementsByTagName("BANKMSGSRSV1").Count > 0)
            {
                _ofxType = "bank";
                return;
            }

            _ofxType = "";
        }

        private void SetStatementNodePaths()
        {
            if (_ofxType == "bank")
            {
                _ofxPath["balanceStatus"] = _ofxPath["bankBalanceStatus"];
                _ofxPath["balanceData"] = _ofxPath["bankBalanceData"];
                _ofxPath["statementStatus"] = _ofxPath["bankStatementStatus"];
                _ofxPath["statementData"] = _ofxPath["bankStatementData"];
                return;
            }

            if(_ofxType == "cc")
            {
                _ofxPath["balanceStatus"] = _ofxPath["ccBalanceStatus"];
                _ofxPath["balanceData"] = _ofxPath["ccBalanceData"];
                _ofxPath["statementStatus"] = _ofxPath["ccStatementStatus"];
                _ofxPath["statementData"] = _ofxPath["ccStatementData"];
                return;
            }
        }

        /*
         *    S I G N O N
         */
        private void ParseSignOn()
        {
            #region OFX sign on response sample
            /*
             *
                <OFX>
                    <SIGNONMSGSRSV1>
                        <SONRS>
                            <STATUS>
                                <CODE>0
                                <SEVERITY>INFO
                                <MESSAGE>The operation succeeded.
                            </STATUS>
                            <DTSERVER>20161006125453.101[-7:PDT]
                            <LANGUAGE>ENG
                            <FI>
                                <ORG>First Tech Federal Credit Union
                                <FID>3169
                            </FI>
                        </SONRS>
                    </SIGNONMSGSRSV1>
                </OFX>
             * 
             */
            #endregion

            // set base location to relevent signon info
            XmlNode status = _doc.SelectSingleNode(_ofxPath["signOnStatus"]);

            // make sure there is a signon block
            if (status == null)
            {
                _signOnRequest.Status = false;
                return;
            }

            // parse signon info
            ParseStatus(status, _signOnRequest);
        }

        /*
         *    A C C O U N T  L I S T
         */
        private void ParseAccountList()
        {
            // parse status
            ParseAccountListStatus();

            // parse data
            if (_accountListRequest.Status)
            {
                ParseAccountListData();
            }
        }

        private void ParseAccountListStatus()
        {
            // get status node
            XmlNode status = _doc.SelectSingleNode(_ofxPath["accountListStatus"]);

            // leave status as null if node is not found
            if (status == null)
            {
                return;
            }

            // parse status node
            ParseStatus(status, _accountListRequest);
        }

        private void ParseAccountListData()
        {
            #region OFX Account List Response Sample
            /*
             *  C R E D I T  C A R D
             *  
                <OFX>
                    <SIGNUPMSGSRSV1>
                        <ACCTINFOTRNRS>
                            <TRNUID>1001
                            <STATUS>
                                <CODE>0
                                <SEVERITY>INFO
                            </STATUS>
                            <ACCTINFORS>
                                <DTACCTUP>20161017093512.346[-4:EDT]
                                <ACCTINFO>
                                    <DESC>CREDIT CARD
                                    <CCACCTINFO>
                                        <CCACCTFROM>
                                            <ACCTID>1234567890
                                        </CCACCTFROM>
                                        <SUPTXDL>Y
                                        <XFERSRC>N
                                        <XFERDEST>N
                                        <SVCSTATUS>ACTIVE
                                    </CCACCTINFO>
                                </ACCTINFO>
                            </ACCTINFORS>
                        </ACCTINFOTRNRS>
                    </SIGNUPMSGSRSV1>
                </OFX>
             *
             * B A N K
             * 
                <OFX>
                    <SIGNUPMSGSRSV1>
                        <ACCTINFOTRNRS>
                            <TRNUID>1001
                            <STATUS>
                                <CODE>0
                                <SEVERITY>INFO
                            </STATUS>
                            <ACCTINFORS>
                                <DTACCTUP>20161006125454.929[-7:PDT]
                                <ACCTINFO>
                                    <DESC>This Account Name
                                    <BANKACCTINFO>
                                        <BANKACCTFROM>
                                            <BANKID>1234567890
                                            <BRANCHID>00
                                            <ACCTID>1234567890
                                            <ACCTTYPE>SAVINGS
                                        </BANKACCTFROM>
                                        <SUPTXDL>Y
                                        <XFERSRC>Y
                                        <XFERDEST>Y
                                        <SVCSTATUS>ACTIVE
                                    </BANKACCTINFO>
                                </ACCTINFO>
                            </ACCTINFORS>
                        </ACCTINFOTRNRS>
                    </SIGNUPMSGSRSV1>
                </OFX>
             */
            #endregion

            // create new XML Document containing only the tag of interest
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(_doc.SelectSingleNode(_ofxPath["accountListData"]).ToString());

            // get all account elements
            XmlNodeList accounts = doc.GetElementsByTagName("ACCTINFO");

            // populate list of account models
            foreach (XmlNode a in accounts)
            {

            }
        }

        /*
         *    B A L A N C E
         */
        private void ParseBalance()
        {
            // parse status
            ParseBalanceStatus();

            // parse balance
            if (_balanceRequest.Status)
            {
                ParseBalanceData();
            }
        }

        private void ParseBalanceStatus()
        {
            // get status node
            XmlNode status = _doc.SelectSingleNode(_ofxPath["balanceStatus"]);

            // leave status as null if node is not found
            if (status == null)
            {
                return;
            }

            // parse status node
            ParseStatus(status, _balanceRequest);
        }

        private void ParseBalanceData()
        {
            #region OFX Balance Response Sample
            /*
             *  C R E D I T  C A R D
             * 
                <OFX>
                    <CREDITCARDMSGSRSV1>
                        <CCSTMTTRNRS>
                            <TRNUID>1001
                            <CCSTMTRS>
                                <CURDEF>USD
                                <CCACCTFROM>
                                    <ACCTID>1234567890
                                </CCACCTFROM>
                                <LEDGERBAL>
                                    <BALAMT>-123.45
                                    <DTASOF>20161017080000.000[-4:EDT]
                                </LEDGERBAL>
                                <AVAILBAL>
                                    <BALAMT>123.45
                                    <DTASOF>20161017080000.000[-4:EDT]
                                </AVAILBAL>
                            </CCSTMTRS>
                        </CCSTMTTRNRS>
                    </CREDITCARDMSGSRSV1>
                </OFX>
             * 
             * B A N K
             *  
                <OFX>
                    <BANKMSGSRSV1>
                        <STMTTRNRS>
                            <TRNUID>1001
                            <STMTRS>
                                <CURDEF>USD
                                <BANKACCTFROM>
                                    <BANKID>1234567890
                                    <ACCTID>1234567890
                                    <ACCTTYPE>CHECKING
                                </BANKACCTFROM>
                                <LEDGERBAL>
                                    <BALAMT>123.45
                                    <DTASOF>20161006125942.700[-7:PDT]
                                </LEDGERBAL>
                            </STMTRS>
                        </STMTTRNRS>
                    </BANKMSGSRSV1>
                </OFX>
             */
            #endregion

            // get node of interest
            XmlNode node = _doc.SelectSingleNode(_ofxPath["bankBalanceData"]);

            // get values from node
            decimal balance = decimal.Parse(node.SelectSingleNode("BALAMT").Value);
            DateTime date = DateTime.Parse(node.SelectSingleNode("DTASOF").Value);

            // populate balance model

        }

        /*
         *    S T A T E M E N T
         */
        private void ParseStatement()
        {
            // parse status
            ParseStatementStatus();

            // parse statement
            if (_statmentRequest.Status)
            {
                ParseStatementData();
            }
        }

        private void ParseStatementStatus()
        {
            // get status node
            XmlNode status = _doc.SelectSingleNode(_ofxPath["statementStatus"]);

            // leave status as null if node is not found
            if (status == null)
            {
                return;
            }

            // parse status node
            ParseStatus(status, _statmentRequest);
        }

        private void ParseStatementData()
        {
            #region OFX Statement Response
            /*
             *  C R E D I T  C A R D
             * 
                <OFX>
                    <CREDITCARDMSGSRSV1>
                        <CCSTMTTRNRS>
                            <TRNUID>1001
                            <CCSTMTRS>
                                <CURDEF>USD
                                <CCACCTFROM>
                                    <ACCTID>1234567890
                                </CCACCTFROM>
                                <BANKTRANLIST>
                                    <DTSTART>20160829200000.000[-4:EDT]
                                    <DTEND>20160901200000.000[-4:EDT]
                                    <STMTTRN>
                                        <TRNTYPE>DEBIT
                                        <DTPOSTED>20160831120000[0:GMT]
                                        <TRNAMT>-123.45
                                        <FITID>1234567890
                                        <NAME>Amazon.com
                                    </STMTTRN>
                                </BANKTRANLIST>
                            </CCSTMTRS>
                        </CCSTMTTRNRS>
                    </CREDITCARDMSGSRSV1>
                </OFX>
             * 
             * B A N K
             * 
                <OFX>
                    <BANKMSGSRSV1>
                        <STMTTRNRS>
                            <TRNUID>1001
                            <STATUS>
                                <CODE>0
                                <SEVERITY>INFO
                            </STATUS>
                            <STMTRS>
                                <CURDEF>USD
                                <BANKACCTFROM>
                                    <BANKID>321180379
                                    <ACCTID>1234567890
                                    <ACCTTYPE>CHECKING
                                </BANKACCTFROM>
                                <BANKTRANLIST>
                                    <DTSTART>20160831170000.000[-7:PDT]
                                    <DTEND>20160930170000.000[-7:PDT]
                                    <STMTTRN>
                                        <TRNTYPE>DEBIT
                                        <DTPOSTED>20160901120000.000
                                        <DTAVAIL>20160901120000.000
                                        <TRNAMT>-123.45
                                        <FITID>1234567890
                                        <NAME>IN *PIGTAILS  CREWCUT 2219 NW AL
                                        <MEMO>POS Transaction--IN *PIGTAILS  CREWCUT 2219 NW ALLIE AVE SUITE503-3364778  O 
                                    </STMTTRN>
                                </BANKTRANLIST>
                            </STMTRS>
                        </STMTTRNRS>
                    </BANKMSGSRSV1>
                </OFX>
             * 
             * 
             */
            #endregion

            // create new XML Document containing only the tag of interest
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(_doc.SelectSingleNode(_ofxPath["statementData"]).ToString());

            // get all account elements
            XmlNodeList transactions = doc.GetElementsByTagName("STMTTRN");

            // populate list of transaction models
            foreach (XmlNode t in transactions)
            {

            }
        }

        /*
         *    S T A T U S
         */
        private void ParseStatus(XmlNode doc, OFXResponseStatus status)
        {
            #region OFX Response Status Samples
            /*
             * S I G N O N
                <OFX>
                    <SIGNONMSGSRSV1>
                        <SONRS>
                            <STATUS>
                                <CODE>0
                                <SEVERITY>INFO
                                <MESSAGE>The operation succeeded.
                            </STATUS>
                            <DTSERVER>20161006123316.060[-7:PDT]
                            <LANGUAGE>ENG
                            <FI>
                                <ORG>First Tech Federal Credit Union
                                <FID>3169
                            </FI>
                        </SONRS>
                    </SIGNONMSGSRSV1>
                </OFX>
             *
             * A C C O U N T  L I S T
             * 
                <OFX>
                    <SIGNUPMSGSRSV1>
                        <ACCTINFOTRNRS>
                            <TRNUID>1001
                            <STATUS>
                                <CODE>0
                                <SEVERITY>INFO
                            </STATUS>
                        </ACCTINFOTRNRS>
                    </SIGNUPMSGSRSV1>
                <OFX>
             * 
             *  B A N K  S T A T E M E N T  A N D  B A L A N C E
             * 
                <OFX>
                    <BANKMSGSRSV1>
                        <STMTTRNRS>
                            <TRNUID>1001
                            <STATUS>
                                <CODE>0
                                <SEVERITY>INFO
                            </STATUS>
                        <STMTTRNRS>
                    </BANKMSGSRSV1>
                </OFX>

             * 
             *  C R E D I T  C A R D   S T A T E M E N T  A N D  B A L A N C E
             * 
                <OFX>
                    <CREDITCARDMSGSRSV1>
                        <CCSTMTTRNRS>
                            <TRNUID>1001
                            <STATUS>
                                <CODE>0
                                <SEVERITY>INFO
                            </STATUS>
                        </CCSTMTTRNRS>
                    </CREDITCARDMSGSRSV1>
                </OFX>
             */
            #endregion

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