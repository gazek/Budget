using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Budget.API.Services
{
    public class TransactionImporter : ITransactionImporter
    {
        public IList<TransactionModel> Transactions { get; private set; }
        // keep track of which transactions already are in the context 
        private IList<int> TransactionsInContext;
        private AccountModel _account;
        private IApplicationDbContext _dbContext;
        private ITransactionDefaults transDefaults;

        // General transaction default fields
        private List<string> updateFieldNames = new List<string>() {"_LastEditDate", "_DateAdded", "_Status"};
        private List<FieldInfo> updateFields;
        private DateTime? _LastEditDate;
        private DateTime? _DateAdded;
        private TransactionStatus? _Status;

        // toggle if details are populated when defaults are applied
        private bool _addDetails;

        public TransactionImporter(IList<TransactionModel> transactions, AccountModel account, IApplicationDbContext dbContext)
        {
            // initialize everything except TransactionDefaults class
            _TransactionImporter(transactions, account, dbContext);
            // initialize TransactionDefaults class
            transDefaults = new TransactionDefaults(_dbContext, ModelMapper.GetUserId(account, _dbContext));
        }

        // just needed for testing
        public TransactionImporter(IList<TransactionModel> transactions, AccountModel account, IApplicationDbContext dbContext,
            ITransactionDefaults defaults)
        {
            // initialize everything except TransactionDefaults class
            _TransactionImporter(transactions, account, dbContext);
            // initialize TransactionDefaults class
            transDefaults = defaults;
        }

        #region Constructor helpers
        private void _TransactionImporter(IList<TransactionModel> transactions, AccountModel account, IApplicationDbContext dbContext)
        {
            // strore DbContext
            _dbContext = dbContext;
            // store account
            _account = account;
            // Set updateFields
            SetUpdateFields();
            // store transactions
            Transactions = new List<TransactionModel>(transactions);
            // initialize TransactionIsInContext field
            TransactionsInContext = new List<int>();
            // Process transaction
            // - add account to transaction
            // - store transactions
            // - determine if they are new or already exist in DbContext
            ProcessTransactions();
        }

        private void ProcessTransactions()
        {
            for (int i = 0; i < Transactions.Count; ++i)
            {
                // Add account
                Transactions[i].Account = _account;
                // add accountId
                Transactions[i].AccountId = _account.Id;
                // Determine which transactions are already in the DbContext
                PopulateTransactionIsInContextField(i);
            }
        }

        private void PopulateTransactionIsInContextField(int i)
        {
            // grab current transaction
            TransactionModel t = Transactions[i];
            // determine if transaction is in DB context
            TransactionModel record = _dbContext.Transactions
                .Where(x => x.ReferenceValue == t.ReferenceValue)
                .Where(x => x.Date == t.Date)
                .FirstOrDefault();
            // add index to TransactionsInContext field
            // and add ID to transaction
            if (record != null)
            {
                TransactionsInContext.Add(i);
                Transactions[i].Id = record.Id;
            }
        }

        private void SetUpdateFields()
        {
            // use reflection to get fields that require updating
            // get all fields in TransactionImporter
            FieldInfo[] allFields = typeof(TransactionImporter).GetFields(
                         BindingFlags.NonPublic |
                         BindingFlags.Instance);
            // find update fields  from list of field names
            updateFields = allFields.Where(x => updateFieldNames.Contains(x.Name)).ToList();
        }
        #endregion

        // Remove transactions from property that are already in the DB or in context but not commited
        public ITransactionImporter FilterExisting()
        {
            // remove transaction from Transactions property
            foreach (int i in TransactionsInContext.Reverse())
            {
                Transactions.RemoveAt(i);
            }
            // Clear TransactionsInContext
            TransactionsInContext.Clear();
            return this;
        }

        #region LastEditDate
        // Use default value
        public ITransactionImporter SetDefaultLastEditDate()
        {
            _LastEditDate = DateTime.Now;
            return this;
        }
        // Use custom value
        public ITransactionImporter SetDefaultLastEditDate(DateTime date)
        {
            _LastEditDate = date;
            return this;
        }
        // do not modify
        public ITransactionImporter UnsetDefaultLastEditDate()
        {
            _LastEditDate = null;
            return this;
        }
        #endregion

        #region DateAdded
        // Use default value
        public ITransactionImporter SetDefaultDateAdded()
        {
            _DateAdded = DateTime.Now;
            return this;
        }
        // Use custom value
        public ITransactionImporter SetDefaultDateAdded(DateTime date)
        {
            _DateAdded = date;
            return this;
        }
        // do not modify
        public ITransactionImporter UnsetDefaultDateAdded()
        {
            _DateAdded = null;
            return this;
        }
        #endregion

        #region Status
        // Use default value
        public ITransactionImporter SetDefaultStatus()
        {
            _Status = TransactionStatus.New;
            return this;
        }
        // Use custom value
        public ITransactionImporter SetDefaultStatus(TransactionStatus status)
        {
            _Status = status;
            return this;
        }
        // do not modify
        public ITransactionImporter UnsetDefaultStatus()
        {
            _Status = null;
            return this;
        }
        #endregion

        #region Details
        public ITransactionImporter SetDefaultDetails()
        {
            _addDetails = true;
            return this;
        }

        public ITransactionImporter UnsetDefaultDetails()
        {
            _addDetails = false;
            return this;
        }
        #endregion

        #region Apply Defaults
        public ITransactionImporter ApplyDefaults()
        {
            // apply general defaults
            ApplyDefaultsGeneral();
            // apply default transaction details
            if (_addDetails == true)
            {
                ApplyDefaultsDetails();
            }
            return this;
        }

        private void ApplyDefaultsGeneral()
        {
            // update each field in each transaction
            foreach (TransactionModel t in Transactions)
            {
                foreach (FieldInfo f in updateFields)
                {
                    if (f.GetValue(this) != null)
                    {
                        typeof(TransactionModel)
                            .GetProperty(f.Name.Substring(1))
                            .SetValue(t, f.GetValue(this));
                    }
                }
            }
        }

        private void ApplyDefaultsDetails()
        {
            // Use TransactionDefaults to get default values for the transactions
            //   then apply to the Transactions field
            foreach (TransactionModel t in Transactions)
            {
                var effectivePayeeName = t.OriginalPayeeName == "" || t.OriginalPayeeName == null ? t.OriginalMemo : t.OriginalPayeeName;
                List<PayeeModel> payees = transDefaults.GetDefaultPayees(effectivePayeeName);
                List<TransactionDetailModel> result = new List<TransactionDetailModel>();
                foreach (PayeeModel p in payees)
                {
                    // get details for current payee
                    List<TransactionDetailModel> details = transDefaults.GetDefaultPayeeDetails(p, t);
                    //Add current payee details to return result
                    if (details.Count > 0)
                    {
                        result.AddRange(details);
                    }
                }
                // store details in transaction
                t.Details = result;
                // Verify sum of details and add uncat to balance if needed
                var detailChecker = new TransactionDetailsChecker(t, _dbContext);
                if (!detailChecker.AmountIsFullyCategorized)
                {
                    t.Details.Add(detailChecker.UncategorizedDetail);
                }
            }
        }
        #endregion

        public int Commit()
        {
            // Add or update DB Context
            foreach (TransactionModel t in Transactions)
            {
                if (t.Id > 0)
                {
                    TransactionModel trans = _dbContext.Transactions.Find(t.Id);
                    trans.LastEditDate = t.LastEditDate;
                    trans.DateAdded = t.DateAdded;
                    trans.Status = t.Status;
                    trans.Details = t.Details;
                }
                else
                {
                    _dbContext.Transactions.Add(t);
                }
            }
            // Save changes
            return _dbContext.SaveChanges();
        }
    }
}