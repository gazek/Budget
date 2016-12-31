using Budget.API.Models;
using Budget.DAL;
using Budget.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budget.API.Services
{
    public class TransactionDetailsChecker
    {
        public TransactionModel Transaction { get; private set; }
        public bool AmountIsFullyCategorized;
        public bool DetailsKeysAreUnique;
        public TransactionDetailModel UncategorizedDetail;
        private IApplicationDbContext _dbContext;
        private decimal _detailsSum;
        HashSet<string> _detailsKeys;

        public TransactionDetailsChecker(TransactionModel trans, IApplicationDbContext dbContext)
        {
            Transaction = trans;
            _dbContext = dbContext;
            ProcessTransaction();
        }

        public TransactionDetailsChecker(TransactionBindingModel trans, IApplicationDbContext dbContext)
        {
            Transaction = ModelMapper.BindingToEntity(trans);
            _dbContext = dbContext;
            ProcessTransaction();
        }

        private void ProcessTransaction()
        {
            TraverseDetails();
            VerifyAmountIsFullyCategorized();
            VerifyDetailsKeysAreUnique();
        }

        private void TraverseDetails()
        {
            _detailsSum = 0m;
            List<TransactionDetailModel> details = Transaction.Details.ToList();
            _detailsKeys = new HashSet<string>();
            for (int i = 0; i < details.Count; ++i)
            {
                _detailsSum += details[i].Amount;
                _detailsKeys.Add(string.Format("{0}_{1}_{2}" , details[i].PayeeId, details[i].CategoryId, details[i].SubCategoryId));
            }
        }

        private void VerifyAmountIsFullyCategorized()
        {
            if (_detailsSum != Transaction.Amount)
            {
                AmountIsFullyCategorized = false;
                InitializeUncatDetail();
                UncategorizedDetail.Amount = Transaction.Amount - _detailsSum;
            }
            else
            {
                AmountIsFullyCategorized = true;
            }
        }

        private void VerifyDetailsKeysAreUnique()
        {
            if (_detailsKeys.Count == Transaction.Details.Count)
            {
                DetailsKeysAreUnique = true;
            }
            else
            {
                DetailsKeysAreUnique = false;
            }
        }

        private void InitializeUncatDetail()
        {
            TransactionDefaults defaults = new TransactionDefaults(_dbContext, Transaction.Account.UserId);
            // initialize uncategorized detail
            UncategorizedDetail = new TransactionDetailModel
            {
                Amount = 0m,
                CategoryId = defaults.GetUncategorizedCat().Id,
                Memo = "Added by system to balance",
                PayeeId = defaults.GetUnassignedPayee().Id,
                SubCategoryId = defaults.GetUncategorizedCat().SubCategories.First().Id,
                LastEditDate = defaults.GetDefaultLastEditDate(),
                TransactionId = Transaction.Id
            };
        }
    }
}