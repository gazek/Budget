using Budget.DAL.Models;
using Budget.API.Models;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Linq;
using Budget.API.Services.OFXClient;
using System;
using System.Collections.Generic;

namespace Budget.API.Services
{
    public class ModelMapper
    {
        #region Account
        public static AccountModel BindingToEntity(AccountBindingModel model, IPrincipal user)
        {
            return new AccountModel
            {
                Id = model.Id,
                UserId = user.Identity.GetUserId(),
                FinancialInstitutionId = model.FinancialInstitutionId,
                RoutingNumber = model.RoutingNumber,
                Number = model.Number,
                Name = model.Name,
                Type = model.Type,
                Description = model.Description ?? model.Name,
                Transactions = new List<TransactionModel>(),
                BalanceHistory = new List<BalanceModel>()
            };
        }

        public static AccountViewModel EntityToView(AccountModel model)
        {
            return new AccountViewModel
            {
                Id = model.Id,
                FinancialInstitutionId = model.FinancialInstitutionId,
                RoutingNumber = model.RoutingNumber,
                Number = model.Number,
                Name = model.Name,
                Type = model.Type,
                Description = model.Description,
                Transactions = model.Transactions,
                Balance = model.BalanceHistory?.OrderByDescending(x => x.AsOfDate).FirstOrDefault()
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
        #endregion

        #region FinancialInstitution
        public static FinancialInstitutionModel BindingToEntity(FinancialInstitutionCreateBindingModel model, IPrincipal user)
        {
            byte[] hash = AesService.EncryptStringToBytes(model.Password);
            return new FinancialInstitutionModel
            {
                Id = model.Id,
                Name = model.Name,
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
                Name = model.Name,
                OfxFid = model.OfxFid,
                OfxUrl = model.OfxUrl,
                OfxOrg = model.OfxOrg,
                Username = model.Username,
                CLIENTUID = model.CLIENTUID
            };
        }
        #endregion
    }
}