using Budget.DAL;
using System.Web.Http;
using Budget.DAL.Models;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using Budget.API.Models;
using System.Linq;
using System.Net.Http;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api")]
    public class BalanceController : ControllerBase<BalanceModel>
    {
        public BalanceController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        // PUT - create or replace balance
        [Route("Account/{AccountId}/Balance", Name = "createOrReplaceBalance")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult createOrReplaceBalance(BalanceBindingModel model, int accountId)
        {
            BalanceModel balance = _dbContext.Balances
                .Where(b => b.AccountId == accountId)
                .Where(b => b.AsOfDate == model.AsOfDate)
                .FirstOrDefault();
            if (balance != null)
            {
                return Update<BalanceBindingModel>(balance.Id, model);
            }
            else
            {
                return Create<BalanceBindingModel, AccountModel>(model, accountId);
            }
        }

        // GET - get balance history api/account/{id}/balance?startDate=yyyy-mm-dd&endDate=yyyy-mm-dd
        [Route("Account/{id}/Balance", Name = "GetAccountBalanceHistory")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetBalanceHistory(int id)
        {
            // Parse date range
            var pairs = this.Request.GetQueryNameValuePairs();
            string begin = pairs.Where(p => p.Key == "startDate").FirstOrDefault().Value ?? "";
            string end = pairs.Where(p => p.Key == "endDate").FirstOrDefault().Value ?? "";
            DateTime beginDate, endDate;
            ParseDateRange(begin, end, out beginDate, out endDate);

            // filters
            List<Expression<Func<BalanceModel, bool>>> filters = new List<Expression<Func<BalanceModel, bool>>>();
            filters.Add(b => b.AccountId == id);
            filters.Add(b => b.AsOfDate >= beginDate);
            filters.Add(b => b.AsOfDate <= endDate);

            // verify existence of account
            // and that user is authorized to access it
            GetRecordAndIsAuthorized<AccountModel>(id);

            if (!_requestIsOk)
            {
                return _errorResponse;
            }

            return GetAll<BalanceModel, DateTime>(filters, null, b => b.AsOfDate);
        }
    }
}