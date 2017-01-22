using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Budget.API.Models;
using Budget.DAL;
using Microsoft.AspNet.Identity.Owin;
using Budget.API.Services;
using System.Data.SqlClient;
using Budget.DAL.Models;
using System.Linq;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Infrastructure;
using System;
using Budget.API.Services.OFXClient;
using System.Security.Principal;
using System.Linq.Expressions;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/FinancialInstitution")]
    public class FinancialInstitutionController : ControllerBase<FinancialInstitutionModel>
    {
        public FinancialInstitutionController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        [Route("", Name = "CreateFI")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(FinancialInstitutionCreateBindingModel model)
        {
            Func<int, string> location = x => Url.Link("GetFiById", new { id = x });
            return base.Create<FinancialInstitutionCreateBindingModel, IPrincipal>(model, User);
        }

        [Route("{id}", Name = "GetFiById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            return base.Get(id);
        }

        [Route("", Name = "GetAllFi")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult GetAll()
        {
            string userId = User.Identity.GetUserId();

            // filters
            var filter = new List<Expression<Func<FinancialInstitutionModel, bool>>>();
            filter.Add(fi => fi.UserId == userId);

            return base.GetAll<FinancialInstitutionModel, string>(filter, null, fi => fi.Name);
        }
        
        [Route("{id}", Name = "UpdateFI")]
        [HttpPut]
        [Authorize]
        public override IHttpActionResult Update<FinancialInstitutionUpdateBindingModel>(int id, FinancialInstitutionUpdateBindingModel model)
        {
            return base.Update(id, model);
        }

        [Route("{id}/credentials", Name = "UpdateFILogin")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult UpdateLogin(int id, FinancialInstitutionUpdateLoginBindingModel model)
        {
            VerifyModel();

            var hashedModel = new FinancialInstitutionUpdateLoginBindingModelHashed()
            {
                Username = model.Username,
                PasswordHash = AesService.EncryptStringToBytes(model.Password)
            };

            return Update<FinancialInstitutionUpdateLoginBindingModelHashed>(id, hashedModel);
        }

        [Route("{id}/GetAccountList", Name = "GetAccountListFromBank")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetAccountList(int id)
        {
            GetRecordAndIsAuthorized(id);

            if (!_requestIsOk)
            {
                return _errorResponse;
            }

            // configure the ofx account list request
            OfxClient.RequestConfig.OfxFid = _record.OfxFid;
            OfxClient.RequestConfig.OfxOrg = _record.OfxOrg;
            OfxClient.RequestConfig.Password = AesService.DecryptStringFromBytes(_record.PasswordHash);
            OfxClient.RequestConfig.RequestType = OFXRequestConfigRequestType.AccountList;
            OfxClient.RequestConfig.URL = new Uri(_record.OfxUrl);
            OfxClient.RequestConfig.Username = _record.Username;
            Guid clientId;
            if (Guid.TryParse(_record.CLIENTUID, out clientId))
            {
                OfxClient.RequestConfig.ClientUID = clientId;
            }

            // Build request
            OfxClient.BuildRequest();

            // Make request
            OfxClient.ExecuteRequest();

            // check request status
            if (OfxClient.Requestor.Status && OfxClient.Requestor.OFX != null)
            {
                OfxClient.ParseResponse();

                if (!OfxClient.Parser.SignOnRequest.Status)
                {
                    return BadRequest(OfxClient.Parser.SignOnRequest.Code + ": " + OfxClient.Parser.SignOnRequest.Message);
                }

                return Ok(OfxClient.Parser.Accounts.Select(x => ModelMapper.EntityToListView(x, id)));
            }

            if (!OfxClient.Requestor.Status)
            {
                return BadRequest(OfxClient.Requestor.ErrorMessage);
            }

            return InternalServerError();
        }

    }
}
