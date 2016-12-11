using Budget.DAL.Models;
using System;
using System.Collections.Generic;

namespace Budget.API.Services
{
    public interface ITransactionImporter
    {
        IList<TransactionModel> Transactions { get; }
        ITransactionImporter FilterExisting();
        ITransactionImporter SetDefaultLastEditDate();
        ITransactionImporter SetDefaultLastEditDate(DateTime date);
        ITransactionImporter UnsetDefaultLastEditDate();
        ITransactionImporter SetDefaultDateAdded();
        ITransactionImporter SetDefaultDateAdded(DateTime date);
        ITransactionImporter UnsetDefaultDateAdded();
        ITransactionImporter SetDefaultStatus();
        ITransactionImporter SetDefaultStatus(TransactionStatus status);
        ITransactionImporter UnsetDefaultStatus();
        ITransactionImporter SetDefaultDetails();
        ITransactionImporter UnsetDefaultDetails();
        ITransactionImporter ApplyDefaults();
        int Commit();
    }
}
