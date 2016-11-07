using Budget.DAL.Models;
using Budget.API.Models;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Security.Cryptography;

namespace Budget.API.Services
{
    public class ModelMapper
    {
        // TODO: Add real values for these
        private static byte[] Key = new byte[32];
        private static byte[] IV = new byte[16];

        public static FinancialInstitutionModel BindingToEntity(FinancialInstitutionCreateBindingModel model, IPrincipal user)
        {
            byte[] hash = AesService.EncryptStringToBytes(model.Password, Key, IV);
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