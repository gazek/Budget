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
        // This should probably take accept optional date range arguments, but maybe later...
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

            List<BalanceModel> balances = _dbContext.Balances.Where(b => b.AccountId == account.Id).OrderByDescending(b => b.AsOfDate).ToList();

            List<BalanceViewModel> result = balances?.Select(x => ModelMapper.EntityToView(x)).ToList();
            
            return Ok(result);
        }

        // GET - OFX request to pull latest transactions api/account/{id}/updatetransactions
        [Route("{id}/PullLatestTransactions", Name = "PullLatestTransactions")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult PullLatestTransactions(int id)
        {
            // look for FI record
            AccountModel entity = _dbContext.Accounts.Find(id);

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

            // Configure request
            ConfigureOfxStatementRequest(entity);

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

                // Add transactions to context
                OfxClient.Parser.StatementTransactions.ForEach(t => t.AccountId = entity.Id);
                foreach (TransactionModel t in OfxClient.Parser.StatementTransactions)
                {
                    TransactionModel record = _dbContext.Transactions
                        .Where(x => x.AccountId == t.AccountId)
                        .Where(x => x.ReferenceValue == t.ReferenceValue)
                        .Where(x => x.Date == t.Date)
                        .FirstOrDefault();
                    if (record == null)
                    {
                        _dbContext.Transactions.Add(t);
                    }
                }
                

                // commit changes
                try
                {
                    int result = _dbContext.SaveChanges();
                    return Ok(result);
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    return BadRequest(ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    return GetErrorResult(ex);
                }
            }

            if (!OfxClient.Requestor.Status)
            {
                return BadRequest(OfxClient.Requestor.ErrorMessage);
            }

            return InternalServerError();
        }

        private IHttpActionResult GetErrorResult(DbUpdateException ex, int allowEx = -1, IHttpActionResult returnIfAllowed = null)
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
                if (sqlEx.Number == allowEx)
                {

                }
                return BadRequest(errors[sqlEx.Number]);
            }

            return BadRequest(exception.Message);
        }

        private void ConfigureOfxStatementRequest(AccountModel entity)
        {
            // configure the ofx statement list request
            OfxClient.RequestConfig.RequestType = OFXRequestConfigRequestType.Statement;
            OfxClient.RequestConfig.Username = entity.FinancialInstitution.Username;
            OfxClient.RequestConfig.Password = AesService.DecryptStringFromBytes(entity.FinancialInstitution.PasswordHash);
            OfxClient.RequestConfig.OfxOrg = entity.FinancialInstitution.OfxOrg;
            OfxClient.RequestConfig.OfxFid = entity.FinancialInstitution.OfxFid;
            OfxClient.RequestConfig.AccountNumber = entity.Number;
            if (entity.RoutingNumber != 0)
            {
                OfxClient.RequestConfig.RoutingNumber = entity.RoutingNumber;
            }
            OfxClient.RequestConfig.AccountType = ModelMapper.Type(entity.Type);
            OfxClient.RequestConfig.URL = new Uri(entity.FinancialInstitution.OfxUrl);
            // Need to add a last updated column to AccountModel
            // Need to set a start date for the account for initial transaction pull
            // for now, just request a month of transactions
            OfxClient.RequestConfig.StartDate = DateTime.Today.AddMonths(-1);
            OfxClient.RequestConfig.EndDate = DateTime.Today;
            Guid clientId;
            if (Guid.TryParse(entity.FinancialInstitution.CLIENTUID, out clientId))
            {
                OfxClient.RequestConfig.ClientUID = clientId;
            }
        }
    }
}
