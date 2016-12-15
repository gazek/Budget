using Budget.API.Models;
using Budget.API.Services;
using Budget.API.Services.OFXClient;
using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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


        // update account properties when owned by user
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

        // GET - get balance history api/account/{id}/balance
        [Route("{id}/Balance/{begin?}/{end?}", Name = "GetAccountBalanceHistory")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetBalanceHistory(int id, string begin = "", string end = "")
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

        // Get - query transactions in DB by date range api/account/{id}/Transactions/Date/{end}/{begin}
        [Route("{id:int}/Transactions/Date/{begin}/{end?}", Name = "QueryTransactions")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetTransactionsFromDb(int id, string begin = "", string end = "")
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

            // Parse date range
            DateTime beginDate, endDate;
            IHttpActionResult parseResult = ParseDateRange(begin, end, out beginDate, out endDate);
            if (parseResult != null)
            {
                return parseResult;
            }

            // make sure the begin is earlier than end
            if (beginDate > endDate)
            {
                return BadRequest("End date must be later than begin date");
            }

            // query transactions
            List<TransactionModel> result = _dbContext.Transactions
                .Where(t => t.AccountId == id)
                .Where(t => t.Date >= beginDate)
                .Where(t => t.Date <= endDate)
                .Include(t => t.Details)
                .OrderBy(t => t.Date)
                .ToList();

            // return result
            return Ok(result);
        }

        // Post - OFX request to pull latest transactions api/account/{id}/Transactions/Date/{end}/{begin}
        [Route("{id:int}/Transactions/Date/{begin}/{end?}", Name = "PullLatestTransactions")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult GetTransactionsFromBank(int id, string begin = "", string end = "")
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

            // Parse date range
            DateTime beginDate, endDate;
            IHttpActionResult parseResult = ParseDateRange(begin, end, out beginDate, out endDate);
            if (parseResult != null)
            {
                return parseResult;
            }

            // make sure the begin is earlier than end
            if (beginDate > endDate)
            {
                return BadRequest("End date must be later than begin date");
            }

            // Configure request
            ConfigureOfxStatementRequest(entity, beginDate, endDate);

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

                // Set transaction default field values
                TransactionImporter importer = new TransactionImporter(OfxClient.Parser.StatementTransactions, entity, _dbContext);
                importer.FilterExisting()
                    .SetDefaultDateAdded()
                    .SetDefaultLastEditDate()
                    .SetDefaultStatus()
                    .SetDefaultDetails()
                    .ApplyDefaults();

                // update balance since we have the data
                if (OfxClient.Parser.BalanceRequest.Status)
                {
                    var bals = _dbContext.Balances.ToList();
                    OfxClient.Parser.Balance.AccountId = id;
                    BalanceModel bal = _dbContext.Balances
                        .Where(b => b.AccountId == id)
                        .Where(b => b.AsOfDate == OfxClient.Parser.Balance.AsOfDate)
                        .FirstOrDefault();
                    if (bal == null)
                    {
                        _dbContext.Balances.Add(OfxClient.Parser.Balance);
                    }
                    else
                    {
                        bal.Amount = OfxClient.Parser.Balance.Amount;
                    }
                }

                // commit changes
                try
                {
                    int count = importer.Commit();
                    return Ok(importer.Transactions.OrderBy(t => t.Date).ToList());
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

        // TODO:
        //   Add routes to query by: amount
        //                           payee
        //                           category
        //                           subcategory
        //  Should this API be under account or transactions or both?

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

        private void ConfigureOfxStatementRequest(AccountModel entity, DateTime begin, DateTime end)
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
            OfxClient.RequestConfig.StartDate = begin;
            OfxClient.RequestConfig.EndDate = end;
            Guid clientId;
            if (Guid.TryParse(entity.FinancialInstitution.CLIENTUID, out clientId))
            {
                OfxClient.RequestConfig.ClientUID = clientId;
            }
        }

        private IHttpActionResult ParseDateRange(string begin, string end, out DateTime beginDate, out DateTime endDate)
        {
            // parse begin date
            DateTime.TryParse(begin, out beginDate);

            // parse end date
            if (end == "")
            {
                endDate = DateTime.Today;
            }
            else
            {
                DateTime.TryParse(end, out endDate);
            }

            if (beginDate == DateTime.MinValue && endDate != DateTime.MinValue)
            {
                return BadRequest("Invalid begin date");
            }

            if (endDate == DateTime.MinValue && beginDate != DateTime.MinValue)
            {
                return BadRequest("Invalid end date");
            }

            if (endDate == DateTime.MinValue && beginDate == DateTime.MinValue)
            {
                return BadRequest("Invalid begin and end dates");
            }

            return null;
        }
    }
}
