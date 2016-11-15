using Budget.API.Models;
using Budget.API.Services;
using Budget.API.Services.OFXClient;
using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private IApplicationDbContext _dbContext;
        private ApplicationUserManager _userManager;
        public IOfxClient OfxClient { get; set; }

        public AccountController(IApplicationDbContext dbContext)
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

        // create account
        [Route("", Name = "CreateAccount")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(AccountBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AccountModel entity = ModelMapper.BindingToEntity(model, User);

            AccountModel record = _dbContext.Accounts.Add(entity);
            try
            {
                int result = _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return GetErrorResult(ex);
            }

            AccountViewModel viewmodel = ModelMapper.EntityToView(record);

            return Created(Url.Link("GetAccountById", new { id = record.Id }), viewmodel);
        }

        // get account by ID
        [Route("{id}", Name = "GetAccountById")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult Get(int id)
        {
            AccountModel entity = _dbContext.Accounts.Find(id);

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

        // get all accounts owned by user
        [Route("", Name = "GetAllAccounts")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult Get()
        {
            string userId = User.Identity.GetUserId();
            List<AccountModel> entities = _dbContext.Accounts
                .Where(x => x.UserId == userId)
                .ToList();

            List<AccountViewModel> result = entities.Select(x => ModelMapper.EntityToView(x)).ToList();

            return Ok(result);
        }


        // update account when owned by user
        [Route("{id}", Name = "UpdateAccount")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult Update(int id, AccountBindingModel model)
        {
            // look for record
            AccountModel record = _dbContext.Accounts.Find(id);

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

        // GET - get balance history api/account/{id}/balancehistory
        [Route("{id}/GetBalanceHistory", Name = "GetAccountBalanceHistory")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetBalanceHistory(int id)
        {
            AccountModel account = _dbContext.Accounts.Find(id);

            if (account == null)
            {
                return NotFound();
            }

            if (account.UserId != User.Identity.GetUserId())
            {
                return Unauthorized();
            }

            List<BalanceModel> balances = account.BalanceHistory?.OrderByDescending(x => x.AsOfDate).ToList();

            List<BalanceViewModel> result = balances?.Select(x => ModelMapper.EntityToView(x)).ToList();
            
            return Ok(result);
        }

        // GET - OFX request to pull latest transactions api/account/{id}/updatetransactions
        [Route("{id}/PullLatestTransactions", Name = "PullLatestTransactions")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult PullLatestTransactions(int id)
        {
            var ex =  new NotImplementedException();
            return InternalServerError(ex);
        }

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
