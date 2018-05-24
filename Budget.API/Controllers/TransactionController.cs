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
using System.Threading.Tasks;

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
            return Update<TransactionBindingModel, TransactionViewModel>(id, model);
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

        // Post - OFX request to pull latest transactions
        // api/account/{id}/Transactions?startDate=yyyy-mm-dd&endDate=yyyy-mm-dd&source=<bank||file>
        [Route("Account/{id:int}/Transactions", Name = "PullLatestTransactionsFromBank")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult ImportTransactions(int id)
        {
            // verify existence of account
            // and that user is authorized to access it
            GetRecordAndIsAuthorized<AccountModel>(id);

            if (!_requestIsOk)
            {
                return _errorResponse;
            }

            // extract the value of the source parameter
            string source = this.Request.GetQueryNameValuePairs().Where(p => p.Key == "source").FirstOrDefault().Value;

            // check the data source
            if (source == "bank")
            {
                return ImportTransactionsFromBank(id);
            }

            if (source == "file")
            {
                var importTransTask = Task.Run<IHttpActionResult>(async () => await ImportTransactionsFromFile(id));
                var result = importTransTask.Result;
                return result;
            }

            // unexpected source value
            return BadRequest(String.Format("Invalid source value: {0}", source)); 
        }

        async private Task<IHttpActionResult> ImportTransactionsFromFile(int id)
        {
            // extract file from request
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            var file = provider.Contents.First();
            var filename = file.Headers.ContentDisposition.FileName.Trim('\"');

            // store file content as string
            var buffer = await file.ReadAsByteArrayAsync();
            string s = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            var body = OFXUtils.PartitionResponse(s)["body"];

            // instantiate the OfxParser
            OfxClient.Parser = new OFXParser(body);

            // initialize response object
            var response = new OfxTransactionRequestViewModel(OfxClient);

            // parse ofx file
            OfxClient.Parser.Parse();

            // verify account in file is the same as the account referenced in the route URL
            var account = _dbContext.Accounts.Find(id);
            if (!(account.Number.Contains(OfxClient.Parser.StatementAccountNumber) || OfxClient.Parser.StatementAccountNumber.Contains(account.Number)))
            {
                return BadRequest("Account in file does not match requested account");
            }

            // add transactions to DB
            response.Code = ImportTransactions(response);

            // add balance to DB
            response.Code = ImportBalance(response, id);

            // return response
            return Content(response.Code, response);
        }

        public IHttpActionResult ImportTransactionsFromBank(int id)
        {
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
                returnStatusCode = ImportTransactions(response);

                // add balance to DB
                returnStatusCode = ImportBalance(response, id);

                return Content(returnStatusCode, response);
            }

            return InternalServerError();
        }

        private HttpStatusCode ImportTransactions(OfxTransactionRequestViewModel response)
        {
            if (!response.Statement.Status && response.Statement.Code != -1)
            {
                return HttpStatusCode.BadRequest;
            }

            TransactionImporter importer = new TransactionImporter(OfxClient.Parser.StatementTransactions, _record, _dbContext);

            // Set transaction default field values
            if (OfxClient.Parser.StatementRequest.Status)
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

            return HttpStatusCode.OK;
        }

        private HttpStatusCode ImportBalance(OfxTransactionRequestViewModel response, int id)
        {
            if (response.Balance.Code != -1 && !response.Balance.Status)
            {
                return HttpStatusCode.BadRequest;
            }

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

            return HttpStatusCode.OK;
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