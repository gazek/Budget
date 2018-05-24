using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Budget.API.Services
{
    public class TransactionDefaults : ITransactionDefaults
    {
        private IApplicationDbContext _dbContext;
        private string _topPayeeNameForSplitTransactions = "Multiple";
        private string _userId;

        public TransactionDefaults(IApplicationDbContext dbContext, string userId)
        {
            _dbContext = dbContext;
            _userId = userId;
        }

        public TransactionStatus GetDefaultStatus()
        {
            return TransactionStatus.New;
        }

        public DateTime GetDefaultDateAdded()
        {
            return DateTime.Now;
        }

        public DateTime GetDefaultLastEditDate()
        {
            return DateTime.Now;
        }

        public PayeeModel GetUnassignedPayee()
        {
            // The UnCategorized category and Unassigned payee should be created when the user registers
            // and should not be editable by the user through the category or payee API
            return _dbContext.Payees
                    .Where(c => c.UserId == _userId)
                    .Where(c => c.Name == "Unassigned")
                    .FirstOrDefault();
        }

        public CategoryModel GetUncategorizedCat()
        {
            // The UnCategorized category and Unassigned payee should be created when the user registers
            // and should not be editable by the user through the category or payee API
            return _dbContext.Categories
                    .Where(c => c.UserId == _userId)
                    .Where(c => c.Name == "Uncategorized")
                    .Include(c => c.SubCategories)
                    .FirstOrDefault();
        }

        public virtual List<PayeeModel> GetDefaultPayees (string originalPayeeName)
        {
            // initialize return list
            List<PayeeModel> result = new List<PayeeModel>();

            // get all the user's payees
            List<PayeeModel> allPayees = _dbContext.Payees
             .Where(x => x.UserId == _userId)
             .Include(x => x.ImportNames)
             .Include(x => x.DefaultDetails)
             .ToList();

            // walk across all user's payees and walk across
            // all the payee's rename defintions looking for a match
            foreach (PayeeModel p in allPayees)
            {
                foreach (PayeeImportNameModel i in p.ImportNames)
                {
                    if (Regex.IsMatch(originalPayeeName ?? "", i.ImportName, RegexOptions.IgnoreCase))
                    {
                        result.Add(p);
                        break;
                    }
                }
            }

            // return result
            return result;
        }

        public List<TransactionDetailModel> GetDefaultPayeeDetails(PayeeModel payee, TransactionModel transaction = null)
        {
            // initialize return list
            List<TransactionDetailModel> result = new List<TransactionDetailModel>();

            // walk across all payee's default details
            foreach (PayeeDefaultDetailModel d in payee.DefaultDetails)
            {
                // add fields which do not require specific transaction info
                TransactionDetailModel detail = new TransactionDetailModel()
                {
                    CategoryId = d.CategoryId,
                    LastEditDate = GetDefaultLastEditDate(),
                    PayeeId = payee.Id,
                    SubCategoryId = d.SubCategoryId
                };

                // add transaction specific fields
                if (transaction != null)
                {
                    detail.TransactionId = transaction.Id;
                    detail.Amount = transaction.Amount * d.Allocation;
                }

                // add to result list
                result.Add(detail);
            }

            // return result
            return result;
        }

        public string GetTopPayeeName (TransactionModel transaction)
        {
            // create set of all payee names
            HashSet<int> payeeIds = new HashSet<int>(transaction.Details.Select(x => x.PayeeId));

            // check size of set to determine result
            if (payeeIds.Count == 1)
            {
                return _dbContext.Payees.Find(payeeIds.First()).Name;
            }
            else if  (payeeIds.Count > 1)
            {
                return _topPayeeNameForSplitTransactions;
            }
            else
            {
                return "";
            } 
        }
    }
}