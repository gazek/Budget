﻿using System.Collections.Generic;
using Budget.DAL.Models;

namespace Budget.API.Services.OFXClient
{
    public interface IOFXResponseStatus
    {
        int Code { get; set; }
        string Message { get; set; }
        string Severity { get; set; }
        bool Status { get; set; }
    }

    public interface IOFXParser
    {
        IOFXResponseStatus AccountListRequest { get; }
        List<AccountModel> Accounts { get; }
        BalanceModel Balance { get; }
        IOFXResponseStatus BalanceRequest { get; }
        IOFXResponseStatus SignOnRequest { get; }
        List<TransactionModel> StatementTransactions { get; }
        IOFXResponseStatus StatmentRequest { get; }

        void Parse();
    }
}