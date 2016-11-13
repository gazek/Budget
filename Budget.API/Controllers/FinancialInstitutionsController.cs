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

        public FinancialInstitutionsController(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
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
            OfxClient ofx = new OfxClient();
            ofx.RequestConfig.OfxFid = entity.OfxFid;
            ofx.RequestConfig.OfxOrg = entity.OfxOrg;
            ofx.RequestConfig.Password = AesService.DecryptStringFromBytes(entity.PasswordHash);
            ofx.RequestConfig.RequestType = OFXRequestConfigRequestType.AccountList;
            ofx.RequestConfig.URL = new Uri(entity.OfxUrl);
            ofx.RequestConfig.Username = entity.Username;
            Guid clientId;
            if (Guid.TryParse(entity.CLIENTUID, out clientId))
            {
                ofx.RequestConfig.ClientUID = clientId;
            }  

            // Build request
            ofx.BuildRequest();

            // Make request
            ofx.ExecuteRequest();

            // check request status
            if (ofx.Requestor.Status && ofx.Requestor.OFX != null)
            {
                ofx.ParseResponse();

                if (!ofx.Parser.SignOnRequest.Status)
                {
                    return BadRequest(ofx.Parser.SignOnRequest.Code + ": " + ofx.Parser.SignOnRequest.Message);
                }

                return Ok(ofx.Parser.Accounts.Select(x => ModelMapper.EntityToListViewModel(x, id)));
            }

            if (!ofx.Requestor.Status)
            {
                return BadRequest(ofx.Requestor.ErrorMessage);
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
