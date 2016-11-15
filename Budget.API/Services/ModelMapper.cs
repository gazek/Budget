using Budget.DAL.Models;
using Budget.API.Models;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Linq;

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
                Number = model.Number,
                Name = model.Name,
                Type = model.Type,
                Description = model.Description
            };
        }

        public static AccountViewModel EntityToView(AccountModel model)
        {
            return new AccountViewModel
            {
                Id = model.Id,
                FinancialInstitutionId = model.FinancialInstitutionId,
                Number = model.Number,
                Name = model.Name,
                Type = model.Type,
                Description = model.Description,
                Transactions = model.Transactions,
                Balance = model.BalanceHistory?.OrderByDescending(x => x.AsOfDate).FirstOrDefault()
            };
        }

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

        public static AccountListViewModel EntityToListView(AccountModel model, int financialInstitutionId = 0)
        {
            return new AccountListViewModel
            {
                FinancialInstitutionId = financialInstitutionId,
                Number = model.Number,
                Name = model.Name,
                Type = model.Type
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