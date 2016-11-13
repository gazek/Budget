using Budget.DAL.Models;
using Budget.API.Models;
using System.Security.Principal;
using Microsoft.AspNet.Identity;

namespace Budget.API.Services
{
    public class ModelMapper
    {
        public static AccountListViewModel EntityToListViewModel(AccountModel model, int financialInstitutionId = 0)
        {
            return new AccountListViewModel
            {
                FinancialInstitutionId = financialInstitutionId,
                Number = model.Number,
                Name = model.Name,
                Type = model.Type
            };
        }

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
    }
}