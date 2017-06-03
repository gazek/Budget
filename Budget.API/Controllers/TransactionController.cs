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

            AccountModel entity = _record;

            // Parse date range
            var pairs = this.Request.GetQueryNameValuePairs();
            string begin = pairs.Where(p => p.Key == "startDate").FirstOrDefault().Value ?? "";
            string end = pairs.Where(p => p.Key == "endDate").FirstOrDefault().Value ?? "";
            DateTime beginDate, endDate;
            ParseDateRange(begin, end, out beginDate, out endDate);

            // Configure request
            ConfigureOfxStatementRequest(entity, beginDate, endDate);

            // Build request
            OfxClient.BuildRequest();

            // Make request
            OfxClient.ExecuteRequest();

            // initialize response object
            OfxTransactionRequestViewModel response = new OfxTransactionRequestViewModel();
            response.Status = OfxClient.Requestor.Status;
            response.Code = OfxClient.Requestor.StatusCode;
            response.Response = OfxClient.Requestor.Response;
            response.Description = OfxClient.Requestor.StatusDescription;
            response.Message = OfxClient.Requestor.ErrorMessage;
            response.OfxResponse = OfxClient.Requestor.OFX;
            response.MsgSetRequestOfx = OfxClient.RequestBuilder.MsgSet;
            response.SignOn = new OFXReqestResponseViewModel();
            response.Statement = new OFXReqestResponseViewModel();
            response.Balance = new OFXReqestResponseViewModel();

            // check request status
            if (OfxClient.Requestor.Status && OfxClient.Requestor.OFX != null)
            {
                // parse response
                OfxClient.ParseResponse();

                // populate response object
                response.SignOn.Status = OfxClient.Parser.SignOnRequest.Status;
                response.SignOn.Code = OfxClient.Parser.SignOnRequest.Code;
                response.SignOn.Severity = OfxClient.Parser.SignOnRequest.Severity;
                response.SignOn.Message = OfxClient.Parser.SignOnRequest.Message;
                response.Statement.Status = OfxClient.Parser.StatmentRequest.Status;
                response.Statement.Code = OfxClient.Parser.StatmentRequest.Code;
                response.Statement.Severity = OfxClient.Parser.StatmentRequest.Severity;
                response.Statement.Message = OfxClient.Parser.StatmentRequest.Message;
                response.Balance.Status = OfxClient.Parser.BalanceRequest.Status;
                response.Balance.Code = OfxClient.Parser.BalanceRequest.Code;
                response.Balance.Severity = OfxClient.Parser.BalanceRequest.Severity;
                response.Balance.Message = OfxClient.Parser.BalanceRequest.Message;

                if (!OfxClient.Parser.SignOnRequest.Status)
                {
                    return Content(HttpStatusCode.BadRequest, response);
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
                    /*
                    var transactions = importer.Transactions
                        .Select(t => ModelMapper.EntityToView(t, _dbContext))
                        .OrderBy(t => t.Date)
                        .ToList();
                    return Ok(transactions);
                    */
                    return Content(HttpStatusCode.OK, response);
                }
                catch (DbUpdateException ex)
                {
                    return GetErrorResult(ex);
                }

            }

            if (!OfxClient.Requestor.Status)
            {
                return Content(HttpStatusCode.BadRequest, response);
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