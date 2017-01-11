using Budget.DAL;
using System.Web.Http;
using Budget.DAL.Models;
using Budget.API.Services;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Data.Entity.Infrastructure;
using Budget.API.Services.OFXClient;
using System.Linq.Expressions;

namespace Budget.API.Controllers
{
    [RoutePrefix("api")]
    [Authorize]
    public class TransactionController : ControllerBase<TransactionModel>
    {
        public TransactionController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        [Authorize]
        [HttpGet]
        [Route("Transaction/{id}", Name = "GetTransaction")]
        public override IHttpActionResult Get(int id)
        {
            // filters
            List<Expression<Func<TransactionModel, bool>>> filters = new List<Expression<Func<TransactionModel, bool>>>();
            filters.Add(t => t.Id == id);

            // include related entities
            List<Expression<Func<TransactionModel, object>>> include = new List<Expression<Func<TransactionModel, object>>>();
            include.Add(t => t.Details.Select(d => d.Payee));
            include.Add(t => t.Details.Select(d => d.Category));
            include.Add(t => t.Details.Select(d => d.SubCategory));

            return base.Get<TransactionModel>(filters, include);
        }

        [Authorize]
        [HttpPut]
        [Route("Transaction/{id}", Name = "UpdateTransaction")]
        public IHttpActionResult Update(int id, TransactionBindingModel model)
        {
            return base.Update<TransactionBindingModel>(id, model);
        }

        // Get - query transactions in DB by date range api/account/{id}/Transactions/Date/{end}/{begin}
        [Route("Account/{id:int}/Transactions/Date/{begin}/{end?}", Name = "QueryTransactions")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetTransactionsFromDb(int id, string begin = "", string end = "")
        {
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

            // filters
            List<Expression<Func<TransactionModel, bool>>> filters = new List<Expression<Func<TransactionModel, bool>>>();
            filters.Add(t => t.AccountId == id);
            filters.Add(t => t.Date >= beginDate);
            filters.Add(t => t.Date <= endDate);

            // get related entities
            var include = new List<string>();
            include.Add("Details");

            // verify existence of account
            // and that user is authorized to access it
            GetRecordAndIsAuthorized<AccountModel>(id);

            if (!_requestIsOk)
            {
                return _errorResponse;
            }

            return GetAll<TransactionModel, DateTime>(filters, include, t => t.Date);
        }

        // Post - OFX request to pull latest transactions api/account/{id}/Transactions/Date/{end}/{begin}
        [Route("Account/{id:int}/Transactions/Date/{begin}/{end?}", Name = "PullLatestTransactions")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult GetTransactionsFromBank(int id, string begin = "", string end = "")
        {
            // verify existence of account
            // and that user is authorized to access it
            GetRecordAndIsAuthorized<AccountModel>(id);

            if (!_requestIsOk)
            {
                return _errorResponse;
            }

            AccountModel entity = _record;
            
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

    }
}