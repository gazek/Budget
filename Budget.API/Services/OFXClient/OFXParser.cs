using System;
using System.Collections.Generic;
using System.Xml;
using Budget.DAL.Models;

namespace Budget.API.Services.OFXClient
{
    public class OFXResponseStatus
    {
        public bool Status { get; set; }
        public int Code { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }

        public OFXResponseStatus()
        {
            Status = false;
            Code = -1;
        }
    }

    public class OFXParser
    {
        // SignOn
        public OFXResponseStatus SignOnRequest { get { return _signOnRequest; } }
        private OFXResponseStatus _signOnRequest;

        // Account List
        public OFXResponseStatus AccountListRequest { get { return _accountListRequest; } }
        private OFXResponseStatus _accountListRequest;
        public List<AccountModel> Accounts { get { return _accounts; } }
        private List<AccountModel> _accounts;

        // Balance
        public OFXResponseStatus BalanceRequest { get { return _balanceRequest; } }
        private OFXResponseStatus _balanceRequest;
        public BalanceModel Balance { get { return _balance; } }
        private BalanceModel _balance;

        // Statment
        public OFXResponseStatus StatmentRequest { get { return _statmentRequest; } }
        private OFXResponseStatus _statmentRequest;
        public List<TransactionModel> StatementTransactions {  get { return _transactions; } }
        private List<TransactionModel> _transactions;

        // internal OFX and XML Doc
        string _ofx;
        XmlDocument _doc;
        string _ofxType = "";

        // paths to XML nodes
        Dictionary<string, string> _ofxPath = new Dictionary<string, string>()
        {
            { "signOnStatus", "OFX/SIGNONMSGSRSV1/SONRS/STATUS" },
            { "accountListStatus", "OFX/SIGNUPMSGSRSV1/ACCTINFOTRNRS/STATUS" },
            { "accountListData", "OFX/SIGNUPMSGSRSV1/ACCTINFOTRNRS" },
            { "bankBalanceStatus", "OFX/BANKMSGSRSV1/STMTTRNRS/STATUS"},
            { "bankBalanceData", "OFX/BANKMSGSRSV1/STMTTRNRS/STMTRS/LEDGERBAL" },
            { "ccBalanceStatus", "OFX/CREDITCARDMSGSRSV1/CCSTMTTRNRS/STATUS" },
            { "ccBalanceData", "OFX/CREDITCARDMSGSRSV1/CCSTMTTRNRS/CCSTMTRS/LEDGERBAL" },
            { "bankStatementStatus", "OFX/BANKMSGSRSV1/STMTTRNRS/STATUS" },
            { "bankStatementData", "OFX/BANKMSGSRSV1/STMTTRNRS/STMTRS/BANKTRANLIST" },
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
            _accounts = new List<AccountModel>();
            _transactions = new List<TransactionModel>();
        }

        public void Parse()
        {
            _doc = new XmlDocument();
            _doc.PreserveWhitespace = true;
            _doc.LoadXml(_ofx);

            // parse signon info
            ParseSignOn();

            // determine request type
            GetOfxType();

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

        private void GetOfxType()
        {
            if (_doc.GetElementsByTagName("CREDITCARDMSGSRSV1").Count > 0)
            {
                _ofxType = "CC";
                return;
            }

            if (_doc.GetElementsByTagName("BANKMSGSRSV1").Count > 0)
            {
                _ofxType = "BANK";
                return;
            }
        }

        private void SetStatementNodePaths()
        {
            if (_ofxType == "BANK")
            {
                _ofxPath["balanceStatus"] = _ofxPath["bankBalanceStatus"];
                _ofxPath["balanceData"] = _ofxPath["bankBalanceData"];
                _ofxPath["statementStatus"] = _ofxPath["bankStatementStatus"];
                _ofxPath["statementData"] = _ofxPath["bankStatementData"];
                return;
            }

            if (_ofxType == "CC")
            {
                _ofxPath["balanceStatus"] = _ofxPath["ccBalanceStatus"];
                _ofxPath["balanceData"] = _ofxPath["ccBalanceData"];
                _ofxPath["statementStatus"] = _ofxPath["ccStatementStatus"];
                _ofxPath["statementData"] = _ofxPath["ccStatementData"];
                return;
            }
        }

        private bool StatusPathExists(string path)
        {
            // verify we have a valid OFX xpath
            // "" is used when a given request type is not part of the response
            if (path == "" || path.IndexOf('/') < 0)
            {
                return false;
            }

            // get tag name one level up from STATUS
            string[] pathElements = path.Split('/');
            string tag = pathElements[pathElements.Length - 2];

            // Look for tag on OFX string
            if (_doc.GetElementsByTagName(tag).Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        private DateTime OfxDateToDateTime(string ofxDate)
        {
            return DateTime.Parse(ofxDate.Substring(0, 4) + "-" + ofxDate.Substring(4, 2) + "-" + ofxDate.Substring(6, 2));
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

            // Check if element exists
            if (!StatusPathExists(_ofxPath["signOnStatus"]))
            {
                _signOnRequest.Status = false;
                return;
            }

            // set base location to relevent signon info
            XmlNode status = _doc.SelectSingleNode(_ofxPath["signOnStatus"]);

            // parse signon info
            ParseStatus(status, _signOnRequest);
        }

        /*
         *    A C C O U N T  L I S T
         */
        private void ParseAccountList()
        {
            // Check if element exists
            if (!StatusPathExists(_ofxPath["accountListStatus"]))
            {
                // leave status as null if node is not found
                return;
            }

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
            doc.LoadXml(_doc.SelectSingleNode(_ofxPath["accountListData"]).OuterXml);

            // get all account elements
            XmlNodeList accounts = doc.GetElementsByTagName("ACCTINFO");

            // populate list of account models
            AccountModel currentAccount;
            string accountType;
            foreach (XmlNode a in accounts)
            {
                // Determine account type
                // account type is used to set xpath
                // It may be possible that both BANKACCTINFO and CCACCTINFO tags
                // could be present in a single account list response so it may
                // not be safe to use _ofxType to set prefix
                string prefix = "";
                if (a.SelectSingleNode("BANKACCTINFO") != null)
                {
                    prefix = "BANK";
                }
                if (a.SelectSingleNode("CCACCTINFO") != null)
                {
                    prefix = "CC";
                }

                // Collect account info fm XML
                currentAccount = new AccountModel();
                currentAccount.Description = a.SelectSingleNode("DESC").InnerText;
                currentAccount.Name = a.SelectSingleNode("DESC").InnerText;
                string path = prefix + "ACCTINFO/" + prefix + "ACCTFROM/";
                currentAccount.Number = a.SelectSingleNode(path + "ACCTID").InnerText;
                if (prefix == "CC")
                {
                    currentAccount.Type = AccountType.CreditCard;
                }
                if (prefix == "BANK")
                {
                    accountType = a.SelectSingleNode(path + "ACCTTYPE").InnerText;
                    if (accountType == "SAVINGS")
                    {
                        currentAccount.Type = AccountType.Savings;
                    }
                    if (accountType == "CHECKING")
                    {
                        currentAccount.Type = AccountType.Checking;
                    }
                }

                // add to list
                _accounts.Add(currentAccount);
            }
        }

        /*
         *    B A L A N C E
         */
        private void ParseBalance()
        {
            // Check if element exists
            if (!StatusPathExists(_ofxPath["balanceStatus"]))
            {
                // leave status as null if node is not found
                return;
            }

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
            XmlNode node = _doc.SelectSingleNode(_ofxPath["balanceData"]);

            // exit if node not found
            if (node == null)
            {
                return;
            }

            // get values from node
            decimal amount = decimal.Parse(node.SelectSingleNode("BALAMT").InnerText);
            DateTime date = OfxDateToDateTime(node.SelectSingleNode("DTASOF").InnerText);

            // populate balance model
            BalanceModel balance = new BalanceModel();
            balance.Amount = amount;
            balance.AsOfDate = date;

            // add balance to property
            _balance = balance;
        }

        /*
         *    S T A T E M E N T
         */
        private void ParseStatement()
        {
            // Check if element exists
            if (!StatusPathExists(_ofxPath["statementStatus"]))
            {
                // leave status as null if node is not found
                return;
            }

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

            // look for statement node
            XmlNode node = _doc.SelectSingleNode(_ofxPath["statementData"]);

            // Verify node was found
            if (node == null)
            {
                return;
            }

            // create new XML Document containing only the tag of interest
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(node.OuterXml);

            // get all account elements
            XmlNodeList transactions = doc.GetElementsByTagName("STMTTRN");

            // populate list of transaction models
            TransactionModel transModel;
            foreach (XmlNode t in transactions)
            {
                // create new class instance
                transModel = new TransactionModel();
                // get amount
                transModel.Amount = decimal.Parse(t.SelectSingleNode("TRNAMT").InnerText);
                // get ref val
                transModel.ReferenceValue = t.SelectSingleNode("FITID").InnerText;
                // get date
                transModel.Date = OfxDateToDateTime(t.SelectSingleNode("DTPOSTED").InnerText);
                // get payee
                transModel.OriginalPayeeName = t.SelectSingleNode("NAME").InnerText;
                // get memo
                transModel.OriginalMemo = t.SelectSingleNode("MEMO").InnerText;
                // add transaction to list
                _transactions.Add(transModel);
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
            if (!int.TryParse(rawStatusCode, out code))
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

            if (status.Code == 0)
            {
                status.Status = true;
            }
        }
    }

}