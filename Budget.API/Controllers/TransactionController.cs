using Budget.DAL;
using System.Web.Http;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using Budget.API.Services;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using System;
using System.Data.Entity.Infrastructure;
using Budget.API.Services.OFXClient;

namespace Budget.API.Controllers
{
    [Authorize]
    public class TransactionController : ControllerBase
    {
        public TransactionController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        [Authorize]
        [HttpGet]
        [Route("Transaction/{id}", Name = "GetTransaction")]
        public IHttpActionResult Get(int id)
        {
            TransactionModel entity = _dbContext.Transactions
                .Where(t => t.Id == id)
                .Include(t => t.Details.Select(d => d.Payee))
                .Include(t => t.Details.Select(d => d.Category))
                .Include(t => t.Details.Select(d => d.SubCategory))
                .Include(t => t.Account)
                .FirstOrDefault();

            if (entity == null)
            {
                return NotFound();
            }

            if (entity.Account.UserId != User.Identity.GetUserId())
            {
                return Unauthorized();
            }

            return Ok(ModelMapper.EntityToView(entity));
        }

        [Authorize]
        [HttpPut]
        [Route("Transaction/{id}", Name = "UpdateTransaction")]
        public IHttpActionResult Update(int id, TransactionBindingModel bindModel)
        {
            // verify user is record owner
            TransactionModel entity = _dbContext.Transactions
                .Where(t => t.Id == id)
                .Include(t => t.Details.Select(d => d.Payee))
                .Include(t => t.Details.Select(d => d.Category))
                .Include(t => t.Details.Select(d => d.SubCategory))
                .Include(t => t.Account)
                .FirstOrDefault();

            if (entity == null)
            {
                return NotFound();
            }

            if (entity.Account.UserId != User.Identity.GetUserId())
            {
                return Unauthorized();
            }

            // verify model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify details
            //   amount must be fully categorized
            //   all detail keys must be unique
            TransactionModel newTrans = ModelMapper.BindingToEntity(bindModel);
            newTrans.Account = entity.Account;
            newTrans.Amount = entity.Amount;
            TransactionDetailsChecker detailsChecker = new TransactionDetailsChecker(newTrans, _dbContext);
            
            // verify that payee, cat and subcat are all unique
            if (detailsChecker.DetailsKeysAreUnique == false)
            {
                return BadRequest("Payee, Category, Subcategory combination must be unique");
            }

            // update db context
            entity.Status = bindModel.Status;
            entity.CheckNum = bindModel.CheckNum;
            entity.Details = bindModel.Details.Select(d => ModelMapper.BindingToEntity(d)).ToList();

            // verify sum of detail amounts equals the transaction amount
            //   add uncategorized detail to balance amount if needed
            if (detailsChecker.AmountIsFullyCategorized == false)
            {
                entity.Details.Add(detailsChecker.UncategorizedDetail);
            }

            // commit changes
            try
            {
                _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return GetErrorResult(ex);
            }

            // return the updated record from the DB
            return Ok(ModelMapper.EntityToView(entity));
        }

        // Get - query transactions in DB by date range api/account/{id}/Transactions/Date/{end}/{begin}
        [Route("Account/{id:int}/Transactions/Date/{begin}/{end?}", Name = "QueryTransactions")]
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
        [Route("Account/{id:int}/Transactions/Date/{begin}/{end?}", Name = "PullLatestTransactions")]
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