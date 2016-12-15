using System;
using System.Collections.Generic;
using Budget.DAL.Models;

namespace Budget.API.Services
{
    public interface ITransactionDefaults
    {
        DateTime GetDefaultDateAdded();
        DateTime GetDefaultLastEditDate();
        List<TransactionDetailModel> GetDefaultPayeeDetails(PayeeModel payee, TransactionModel transaction = null);
        List<PayeeModel> GetDefaultPayees(string userId, string originalPayeeName);
        TransactionStatus GetDefaultStatus();
        string GetTopPayeeName(TransactionModel transaction);
    }
}