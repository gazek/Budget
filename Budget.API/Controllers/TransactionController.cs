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
using Budget.API.Models;
using System.Net;
using System.Net.Http;

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
        [Route("Transaction/{id}", Name = "GetTransactionById")]
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
            return Update<TransactionBindingModel>(id, model);
        }

        // Get - query transactions in DB by date range api/account/{id}/Transactions?startDate=yyyy-mm-dd&endDate=yyyy-mm-dd
        [Route("Account/{id:int}/Transactions", Name = "GetTransactionsByAccountAndDateRange")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetTransactionsFromDb(int id)
        {
            // Parse date range
            var pairs = this.Request.GetQueryNameValuePairs();
            string begin = pairs.Where(p => p.Key == "startDate").FirstOrDefault().Value ?? "";
            string end = pairs.Where(p => p.Key == "endDate").FirstOrDefault().Value ?? "";
            DateTime beginDate, endDate;
            ParseDateRange(begin, end, out beginDate, out endDate);

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

        // Post - OFX request to pull latest transactions api/account/{id}/Transactions?startDate=yyyy-mm-dd&endDate=yyyy-mm-dd
        [Route("Account/{id:int}/Transactions", Name = "PullLatestTransactionsFromBank")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult ImportTransactionsFromBank(int id)
        {
            // verify existence of account
            // and that user is authorized to access it
            GetRecordAndIsAuthorized<AccountModel>(id);

            if (!_requestIsOk)
            {
                return _errorResponse;
            }

            // Parse date range
            DateTime beginDate, endDate;
            GetBeginAndEndDates(out beginDate, out endDate);

            // Configure request
            ConfigureOfxStatementRequest(beginDate, endDate);

            // Build request
            OfxClient.BuildRequest();

            // Make request
            OfxClient.ExecuteRequest();

            // initialize response object
            var response = new OfxTransactionRequestViewModel(OfxClient);

            // check if POST to bank was successful
            if (!OfxClient.Requestor.Status)
            {
                response.Status = false;
                response.Message.Add("Failed to connecto to bank OFX service");
                return Content(HttpStatusCode.ServiceUnavailable, response);
            }

            // check request status
            if (OfxClient.Requestor.Status && OfxClient.Requestor.OFX != null)
            {
                var returnStatusCode = HttpStatusCode.OK;

                // parse response
                OfxClient.ParseResponse();

                // populate response object
                response.PopulateResponse(OfxClient);

                // add transactions to DB
                if (response.Statement.Status)
                {
                    ImportTransactions(response);
                }
                else
                {
                    returnStatusCode = HttpStatusCode.BadRequest;
                }

                // add balance to DB
                if (response.Balance.Status)
                {
                    ImportBalance(response, id);
                }
                else
                {
                    returnStatusCode = HttpStatusCode.BadRequest;
                }

                return Content(returnStatusCode, response);
            }

            return InternalServerError();
        }

        private void ImportTransactions(OfxTransactionRequestViewModel response)
        {
            TransactionImporter importer = new TransactionImporter(OfxClient.Parser.StatementTransactions, _record, _dbContext);

            // Set transaction default field values
            if (OfxClient.Parser.StatmentRequest.Status)
            {
                importer.FilterExisting()
                    .SetDefaultDateAdded()
                    .SetDefaultLastEditDate()
                    .SetDefaultStatus()
                    .SetDefaultDetails()
                    .ApplyDefaults();
            }

            // commit changes
            try
            {
                int count = importer.Commit();
            }
            catch (DbUpdateException ex)
            {
                response.Status = false;
                response.Message.Add("Failed to import transactions");
            }
        }

        private void ImportBalance(OfxTransactionRequestViewModel response, int id)
        {
            // update balance
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

                // commit changes
                try
                {
                    int count = _dbContext.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    response.Status = false;
                    response.Message.Add("Failed to import balance");
                }
            }
        }

        private void GetBeginAndEndDates(out DateTime beginDate, out DateTime endDate)
        {
            var pairs = this.Request.GetQueryNameValuePairs();
            string begin = pairs.Where(p => p.Key == "startDate").FirstOrDefault().Value ?? "";
            string end = pairs.Where(p => p.Key == "endDate").FirstOrDefault().Value ?? "";
            ParseDateRange(begin, end, out beginDate, out endDate);
        }

        private void ConfigureOfxStatementRequest(DateTime begin, DateTime end)
        {
            var entity = _record;
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
            OfxClient.RequestConfig.StartDate = begin;
            OfxClient.RequestConfig.EndDate = end;
            Guid clientId;
            if (Guid.TryParse(entity.FinancialInstitution.CLIENTUID, out clientId))
            {
                OfxClient.RequestConfig.ClientUID = clientId;
            }
        }

        // TODO:
        //   Add routes to query by: amount
        //                           payee
        //                           category
        //                           subcategory
    }
}