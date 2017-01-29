using Budget.DAL.Models;
using Budget.API.Models;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Linq;
using Budget.API.Services.OFXClient;
using System;
using System.Collections.Generic;
using Budget.DAL;
using System.Reflection;
using System.Runtime.Remoting;

namespace Budget.API.Services
{
    /*
     * Maps model to model:
     *      Example:
     *          Binding=>Entity
     *          Entity=>View 
     *          etc.
     * 
     * Maps fields across FK associations: 
     *      Example:
     *          Find UserId associated with transaction
     *              Transaction.AccountId => Account.UserId
     *              TransactionDetail.TransactionId => Transaction.AccountId => Account.UserId
     * 
     */
    public static class ModelMapper
    {
        #region Account
        public static AccountModel BindingToEntity(AccountBindingModel model, FinancialInstitutionModel fiModel)
        {
            return new AccountModel
            {
                FinancialInstitutionId = fiModel.Id,
                RoutingNumber = model.RoutingNumber,
                Number = model.Number,
                Name = string.Join(" ", model.Name.Split(' ')).ToLower(),
                Type = model.Type,
                Description = model.Description ?? model.Name,
                Transactions = new List<TransactionModel>(),
                BalanceHistory = new List<BalanceModel>()
            };
        }

        public static AccountViewModel EntityToView(AccountModel model, IApplicationDbContext dbContext)
        {
            BalanceModel bal = model.BalanceHistory.OrderByDescending(x => x.AsOfDate).FirstOrDefault();

            return new AccountViewModel
            {
                Id = model.Id,
                FinancialInstitutionId = model.FinancialInstitutionId,
                RoutingNumber = model.RoutingNumber,
                Number = model.Number,
                Name = model.Name,
                Type = model.Type,
                Description = model.Description,
                Balance = (bal != null) ? ModelMapper.EntityToView(bal) : null
            };
        }

        public static AccountViewModel EntityToView(AccountModel model)
        {
            BalanceModel bal = model.BalanceHistory.OrderByDescending(x => x.AsOfDate).FirstOrDefault();

            return new AccountViewModel
            {
                Id = model.Id,
                FinancialInstitutionId = model.FinancialInstitutionId,
                RoutingNumber = model.RoutingNumber,
                Number = model.Number,
                Name = model.Name,
                Type = model.Type,
                Description = model.Description,
                Balance = (bal != null) ? ModelMapper.EntityToView(bal) : null
            };
        }

        public static AccountListViewModel EntityToListView(AccountModel model, int financialInstitutionId = 0)
        {
            return new AccountListViewModel
            {
                FinancialInstitutionId = financialInstitutionId,
                RoutingNumber = model.RoutingNumber,
                Number = model.Number,
                Name = model.Name,
                Type = model.Type
            };
        }

        public static OFXRequestConfigAccountType Type(AccountType model)
        {
            switch (model)
            {
                case AccountType.Checking:
                    return OFXRequestConfigAccountType.CHECKING;
                case AccountType.CreditCard:
                    return OFXRequestConfigAccountType.CREDITCARD;
                case AccountType.Savings:
                    return OFXRequestConfigAccountType.SAVINGS;
                default:
                    throw new Exception("Failed to convert AccountType Enum");
            }
        }

        public static AccountType Type(OFXRequestConfigAccountType model)
        {
            switch (model)
            {
                case OFXRequestConfigAccountType.CHECKING:
                    return AccountType.Checking;
                case OFXRequestConfigAccountType.CREDITCARD:
                    return AccountType.CreditCard;
                case OFXRequestConfigAccountType.SAVINGS:
                    return AccountType.Savings;
                default:
                    throw new Exception("Failed to convert AccountType Enum");
            }
        }

        public static string GetUserId(AccountModel model, IApplicationDbContext dbContext)
        {
            FinancialInstitutionModel fi = dbContext.FinancialInstitutions.Find(model.FinancialInstitutionId);
            return fi.UserId;
        }
        #endregion

        #region Balance
        public static BalanceViewModel EntityToView(BalanceModel model)
        {
            return new BalanceViewModel
            {
                Id = model.Id,
                AccountId = model.AccountId,
                Amount = model.Amount,
                AsOfDate = model.AsOfDate
            };
        }

        public static string GetUserId(BalanceModel model, IApplicationDbContext dbContext)
        {
            AccountModel account = dbContext.Accounts.Find(model.AccountId);
            return GetUserId(account, dbContext);
        }
        #endregion

        #region FinancialInstitution
        public static FinancialInstitutionModel BindingToEntity(FinancialInstitutionCreateBindingModel model, IPrincipal user)
        {
            byte[] hash = AesService.EncryptStringToBytes(model.Password);
            return new FinancialInstitutionModel
            {
                Name = string.Join(" ", model.Name.Split(' ')).ToLower(),
                NameStylized = string.Join(" ", model.Name.Split(' ')),
                OfxFid = model.OfxFid,
                OfxUrl = model.OfxUrl,
                OfxOrg = model.OfxOrg,
                Username = model.Username,
                PasswordHash = hash,
                UserId = user.Identity.GetUserId(),
                CLIENTUID = model.CLIENTUID
            };
        }

        public static FinancialInstitutionViewModel EntityToView(FinancialInstitutionModel model)
        {
            return new FinancialInstitutionViewModel
            {
                Id = model.Id,
                Name = model.NameStylized,
                OfxFid = model.OfxFid,
                OfxUrl = model.OfxUrl,
                OfxOrg = model.OfxOrg,
                Username = model.Username,
                CLIENTUID = model.CLIENTUID
            };
        }

        public static string GetUserId(FinancialInstitutionModel model, IApplicationDbContext dbContext = null)
        {
            return model.UserId;
        }
        #endregion

        #region Transaction
        public static TransactionModel BindingToEntity(TransactionBindingModel model)
        {
            return new TransactionModel
            {
                Status = model.Status,
                CheckNum = model.CheckNum
            };
        }

        public static TransactionViewModel EntityToView(TransactionModel model, IApplicationDbContext dbContext)
        {
            // get top fields
            TopFields topFields = new TopFields(model);

            // map details
            List<TransactionDetailViewModel> details = model.Details?.Select(d => ModelMapper.EntityToView(d, dbContext)).ToList();

            return new TransactionViewModel
            {
                Id = model.Id,
                AccountId = model.AccountId,
                ReferenceValue = model.ReferenceValue,
                Date = model.Date,
                Amount = model.Amount,
                PayeeId = topFields.PayeeId,
                PayeeName = topFields.PayeeName,
                CategoryId = topFields.CategoryId,
                CategoryName = topFields.CategoryName,
                SubCategoryId = topFields.SubCategoryId,
                SubCategoryName = topFields.SubCategoryName,
                Memo = topFields.Memo,
                OriginalPayeeName = model.OriginalPayeeName,
                OriginalMemo = model.OriginalMemo,
                DateAdded = model.DateAdded,
                Status = model.Status,
                CheckNum = model.CheckNum,
                LastEditDate = model.LastEditDate,
                Details = details
            };
        }

        public static string GetUserId(TransactionModel model, IApplicationDbContext dbContext = null)
        {
            AccountModel account = dbContext.Accounts.Find(model.AccountId);
            return GetUserId(account, dbContext);
        }

        #region Top Field Helpers
        private class TopFields
        {
            public int PayeeId { get; set; }
            public string PayeeName { get; set; }
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
            public int SubCategoryId { get; set; }
            public string SubCategoryName { get; set; }
            public string Memo { get; set; }

            private ICollection<TransactionDetailModel> details;

            public TopFields(TransactionModel trans)
            {
                details = trans.Details;
                if (details != null)
                {
                    // payee
                    GetTopPayee();
                    // category
                    GetTopCategory();
                    // subcategory
                    GetTopSubCategory();
                    // memo
                    GetTopMemo();
                }
            }

            private void GetTopPayee()
            {
                HashSet<int> idSet = new HashSet<int>(details.Select(d => d.PayeeId));
                // one payee
                if (idSet.Count == 1)
                {
                    PayeeId = idSet.First();
                    PayeeName = details.First().Payee?.NameStylized;
                }
                // more than 1 payee
                if (idSet.Count > 1)
                {
                    PayeeName = "Split";
                }
            }

            private void GetTopCategory()
            {
                HashSet<int> idSet = new HashSet<int>(details.Select(d => d.CategoryId));
                // one category
                if (idSet.Count == 1)
                {
                    CategoryId = idSet.First();
                    CategoryName = details.First().Category?.NameStylized;
                }
                // more than 1 category
                if (idSet.Count > 1)
                {
                    CategoryName = "Split";
                }
            }

            private void GetTopSubCategory()
            {
                HashSet<int> idSet = new HashSet<int>(details.Select(d => d.SubCategoryId));
                // one Subcategory
                if (idSet.Count == 1)
                {
                    SubCategoryId = idSet.First();
                    SubCategoryName = details.First().SubCategory?.NameStylized;
                }
                // more than 1 Subcategory
                if (idSet.Count > 1)
                {
                    SubCategoryName = "Split";
                }
            }

            private void GetTopMemo()
            {
                // set default value
                Memo = "";
                // put memos in a set
                HashSet<string> memoSet = new HashSet<string>(details.Select(d => d.Memo));
                // one memo
                if (memoSet.Count == 1)
                {
                    Memo = memoSet.First();
                    Memo = details.First().Memo;
                }
                // more than 1 Subcategory
                if (memoSet.Count > 1)
                {
                    Memo = "Split";
                }
            }
        }
        #endregion

        #endregion

        #region Transaction Details
        public static TransactionDetailModel BindingToEntity(TransactionDetailBindingModel model)
        {
            return new TransactionDetailModel
            {
                PayeeId = model.PayeeId,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
                Amount = model.Amount,
                TransferMatchId = model.TransferTransactionId,
                Memo = model.Memo,
                LastEditDate = DateTime.Today
            };
        }

        public static TransactionDetailModel BindingToEntity(TransactionDetailBindingModel model, TransactionModel principal)
        {
            return new TransactionDetailModel
            {
                TransactionId = principal.Id,
                PayeeId = model.PayeeId,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
                Amount = model.Amount,
                TransferMatchId = model.TransferTransactionId,
                Memo = model.Memo,
                LastEditDate = DateTime.Today
            };
        }

        public static TransactionDetailViewModel EntityToView(TransactionDetailModel model, IApplicationDbContext dbContext)
        {
            return new TransactionDetailViewModel
            {
                Id = model.Id,
                TransactionId = model.TransactionId,
                PayeeId = model.PayeeId,
                PayeeName = dbContext.Payees.Find(model.PayeeId).NameStylized,
                CategoryId = model.CategoryId,
                CategoryName = dbContext.Categories.Find(model.CategoryId).NameStylized,
                SubCategoryId = model.SubCategoryId,
                SubCategoryName = dbContext.SubCategories.Find(model.SubCategoryId).NameStylized,
                Amount = model.Amount,
                TransferTransactionId = model.TransactionId,
                Memo = model.Memo,
                LastEditDate = model.LastEditDate,
            };
        }

        public static string GetUserId(TransactionDetailModel model, IApplicationDbContext dbContext = null)
        {
            TransactionModel trans = dbContext.Transactions.Find(model.TransactionId);
            return GetUserId(trans);
        }
        #endregion

        #region Category
        public static CategoryModel BindingToEntity(CategoryBindingModel model, IPrincipal user)
        {
            return new CategoryModel()
            {
                UserId = user.Identity.GetUserId(),
                Name = string.Join(" ", model.Name.Split(' ')).ToLower(),
                NameStylized = string.Join(" ", model.Name.Split(' ')),
            };
        }

        public static CategoryViewModel EntityToView(CategoryModel model)
        {
            List<SubCategoryViewModel> subcats;
            if (model.SubCategories == null)
            {
                subcats = new List<SubCategoryViewModel>();
            }
            else
            {
                subcats = new List<SubCategoryViewModel>(model.SubCategories.Select(s => EntityToView(s)));
            }

            return new CategoryViewModel
            {
                Id = model.Id,
                Name = model.NameStylized,
                SubCategories = subcats
            };
        }

        public static string GetUserId(CategoryModel model, IApplicationDbContext dbContext = null)
        {
            return model.UserId;
        }
        #endregion

        #region SubCategory
        public static SubCategoryModel BindingToEntity(SubCategoryBindingModel model, CategoryModel principal)
        {
            return new SubCategoryModel()
            {
                CategoryId = principal.Id,
                Name = string.Join(" ", model.Name.Split(' ')).ToLower(),
                NameStylized = string.Join(" ", model.Name.Split(' ')),
            };
        }

        public static SubCategoryModel BindingToEntity(SubCategoryBindingModel model)
        {
            return new SubCategoryModel()
            {
                CategoryId = model.CategoryId,
                Name = string.Join(" ", model.Name.Split(' ')).ToLower(),
                NameStylized = string.Join(" ", model.Name.Split(' ')),
            };
        }

        public static SubCategoryViewModel EntityToView(SubCategoryModel model)
        {
            return new SubCategoryViewModel
            {
                Id = model.Id,
                CategoryId = model.CategoryId,
                Name = model.NameStylized
            };
        }

        public static string GetUserId(SubCategoryModel model, IApplicationDbContext dbContext = null)
        {
            CategoryModel cat = dbContext.Categories.Find(model.CategoryId);
            return GetUserId(cat);
        }
        #endregion

        #region Payee

        public static string GetUserId(PayeeModel model, IApplicationDbContext dbContext = null)
        {
            return model.UserId;
        }

        public static PayeeModel BindingToEntity(PayeeBindingModel model, IPrincipal principal)
        {
            return new PayeeModel()
            {
                Name = string.Join(" ", model.Name.Split(' ')).ToLower(),
                NameStylized = string.Join(" ", model.Name.Split(' ')),
                UserId = principal.Identity.GetUserId()
            };
        }

        public static PayeeViewModel EntityToView(PayeeModel model, IApplicationDbContext dbContext = null)
        {
            var defaultDetails = dbContext.PayeeDefaultDetails.Where(d => d.PayeeId == model.Id).ToList();
            var importNames = dbContext.PayeeImportNames.Where(d => d.PayeeId == model.Id).ToList();
            return new PayeeViewModel
            {
                Id = model.Id,
                Name = model.NameStylized,
                DefaultDetails = defaultDetails.Select(d => ModelMapper.EntityToView(d, dbContext)).ToList(),
                ImportNames = importNames.Select(i => ModelMapper.EntityToView(i, dbContext)).ToList()
            };
        }
        #endregion

        #region Payee Default Detail

        public static string GetUserId(PayeeDefaultDetailModel model, IApplicationDbContext dbContext = null)
        {
            PayeeModel payee = dbContext.Payees.Find(model.PayeeId);
            return GetUserId(payee);
        }

        public static PayeeDefaultDetailViewModel EntityToView(PayeeDefaultDetailModel model, IApplicationDbContext dbContext = null)
        {
            return new PayeeDefaultDetailViewModel
            {
                Id = model.Id,
                Allocation = model.Allocation,
                PayeeId = model.PayeeId,
                PayeeName = dbContext.Payees.Find(model.PayeeId).NameStylized,
                CategoryId = model.CategoryId,
                CategoryName = dbContext.Categories.Find(model.CategoryId).NameStylized,
                SubCategoryId = model.SubCategoryId,
                SubCategoryName = dbContext.SubCategories.Find(model.SubCategoryId).NameStylized
            };
        }

        public static PayeeDefaultDetailModel BindingToEntity(PayeeDefaultDetailBindingModel model, PayeeModel principal)
        {
            return new PayeeDefaultDetailModel
            {
                Allocation = model.Allocation,
                PayeeId = principal.Id,
                CategoryId = model.CategoryId,
                SubCategoryId = model.SubCategoryId,
            };
        }
        #endregion

        #region Payee Import Name

        public static string GetUserId(PayeeImportNameModel model, IApplicationDbContext dbContext = null)
        {
            PayeeModel payee = dbContext.Payees.Find(model.PayeeId);
            return GetUserId(payee);
        }

        public static PayeeImportNameViewModel EntityToView(PayeeImportNameModel model, IApplicationDbContext dbContext = null)
        {
            return new PayeeImportNameViewModel
            {
                Id = model.Id,
                PayeeId = model.PayeeId,
                PayeeName = dbContext.Payees.Find(model.PayeeId).NameStylized,
                ImportName = model.ImportName
            };
        }
        
        public static PayeeImportNameModel BindingToEntity(PayeeImportNameBindingModel model, PayeeModel principal)
        {
            return new PayeeImportNameModel
            {
                PayeeId = principal.Id,
                ImportName = model.ImportName
            };
        }
        #endregion

        #region Generics
        // 
        public static string GetUserId(dynamic model, IApplicationDbContext dbContext)
        {
            return GetUserId(model, dbContext);
        }

        public static dynamic EntityToView(dynamic model, IApplicationDbContext dbContext)
        {
            string modelType = model.GetType().Name;
            
            IEnumerable<MethodInfo> methods = typeof(ModelMapper)
                .GetMethods().Where(m => m.Name == "EntityToView")
                .Where(m => m.GetParameters().Length.Equals(2))
                .Where(m => m.GetParameters()[1].ParameterType == typeof(IApplicationDbContext))
                .Where(m => modelType.Contains(m.GetParameters()[0].ParameterType.Name));
            if (methods.Count() > 0)
            {
                return EntityToView(model, dbContext);
            }
            else
            {
                return EntityToView(model);
            }
        }

        public static dynamic BindingToEntity(dynamic model)
        {
            return BindingToEntity(model);
        }

        public static dynamic BindingToEntity(dynamic model, dynamic principal)
        {
            return BindingToEntity(model, principal);
        }
        #endregion
    }
}