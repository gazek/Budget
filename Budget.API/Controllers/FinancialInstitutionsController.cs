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

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/FinancialInstitution")]
    public class FinancialInstitutionsController : ApiController
    {
        private IApplicationDbContext _dbContext;
        private ApplicationUserManager _userManager;
        public IOfxClient OfxClient { get; set; }

        public FinancialInstitutionsController(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            OfxClient = new OfxClient();
        }
       
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [Route("", Name = "CreateFI")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(FinancialInstitutionCreateBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = ModelMapper.BindingToEntity(model, User);

            var record = _dbContext.FinancialInstitutions.Add(entity);
            try
            {
                var result = _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return GetErrorResult(ex);
            }

            return Created<FinancialInstitutionViewModel>(Url.Link("GetFiById", new { id = record.Id }), ModelMapper.EntityToView(record));
        }

        [Route("{id}", Name="GetFiById")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult Get(int id)
        {
            FinancialInstitutionModel entity = _dbContext.FinancialInstitutions.Find(id);

            if (entity == null)
            {
                return NotFound();
            }

            if (entity.UserId != User.Identity.GetUserId())
            {
                return Unauthorized();
            }

            return Ok(ModelMapper.EntityToView(entity));
        }

        [Route("", Name = "GetAllFi")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult Get()
        {
            string userId = User.Identity.GetUserId();
            List<FinancialInstitutionModel> entities = _dbContext.FinancialInstitutions
                .Where(x => x.UserId == userId)
                .ToList();

            List<FinancialInstitutionViewModel> result = entities.Select(x => ModelMapper.EntityToView(x)).ToList();

            return Ok(result);
        }
        
        [Route("{id}", Name = "UpdateFI")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult Update(int id, FinancialInstitutionUpdateBindingModel model)
        {
            // look for record
            FinancialInstitutionModel record = _dbContext.FinancialInstitutions.Find(id);

            // check if record exists
            if (record == null)
            {
                return NotFound();
            }

            // check if user owns record
            if (record.UserId != User.Identity.GetUserId())
            {
                return Unauthorized();
            }

            // verify model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // make updates
            foreach (string key in model.GetType().GetProperties().Select(x => x.Name))
            {
                record.GetType().GetProperty(key).SetValue(record, model.GetType().GetProperty(key).GetValue(model));
            }

            try
            {
                // commit changes
                int result = _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return GetErrorResult(ex);
            }
            
            // return updated record
            return Ok();
        }

        [Route("{id}/credentials", Name = "UpdateFILogin")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult UpdateLogin(int id, FinancialInstitutionUpdateLoginBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            FinancialInstitutionModel record = _dbContext.FinancialInstitutions.Find(id);

            if (record == null)
            {
                return NotFound();
            }

            if (record.UserId != User.Identity.GetUserId())
            {
                return Unauthorized();
            }

            record.Username = model.Username;
            record.PasswordHash = AesService.EncryptStringToBytes(model.Password);

            try
            {
                int result = _dbContext.SaveChanges();
                return Ok();
            }
            catch (DbUpdateException ex)
            {
                return GetErrorResult(ex);
            }
        }

        [Route("{id}/GetAccountList", Name = "GetAccountLists")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetAccountList(int id)
        {
            // lool for FI record
            FinancialInstitutionModel entity = _dbContext.FinancialInstitutions.Find(id);

            // return if not found
            if (entity == null)
            {
                return NotFound();
            }

            // return if requestor is not asuthorized
            if (entity.UserId != User.Identity.GetUserId())
            {
                return Unauthorized();
            }
            
            // configure the ofx account list request
            OfxClient.RequestConfig.OfxFid = entity.OfxFid;
            OfxClient.RequestConfig.OfxOrg = entity.OfxOrg;
            OfxClient.RequestConfig.Password = AesService.DecryptStringFromBytes(entity.PasswordHash);
            OfxClient.RequestConfig.RequestType = OFXRequestConfigRequestType.AccountList;
            OfxClient.RequestConfig.URL = new Uri(entity.OfxUrl);
            OfxClient.RequestConfig.Username = entity.Username;
            Guid clientId;
            if (Guid.TryParse(entity.CLIENTUID, out clientId))
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

                return Ok(OfxClient.Parser.Accounts.Select(x => ModelMapper.EntityToListViewModel(x, id)));
            }

            if (!OfxClient.Requestor.Status)
            {
                return BadRequest(OfxClient.Requestor.ErrorMessage);
            }

            return InternalServerError();
        }

        // Get - OFX request for account list from FI
        private IHttpActionResult GetErrorResult(DbUpdateException ex)
        {
            var errors = new Dictionary<int, string>
            {
                { 2601, "Operation failed because record already exists" }
            };

            if (ex.InnerException == null)
            {
                return BadRequest(ex.Message);
            }

            var exception = ex.InnerException;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            SqlException sqlEx = (SqlException)exception;
            if (errors.ContainsKey(sqlEx.Number))
            {
                return BadRequest(errors[sqlEx.Number]);
            }

            return BadRequest(exception.Message);
        }
    }
}
