using Budget.DAL;
using System.Web.Http;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using Budget.API.Models;
using Budget.API.Services;
using System;
using System.Linq.Expressions;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api")]
    public class BalanceController : ControllerBase<BalanceModel>
    {
        public BalanceController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }
        
        // GET - get balance history api/account/{id}/balance
        [Route("Account/{id}/Balance/{begin?}/{end?}", Name = "GetAccountBalanceHistory")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetBalanceHistory(int id, string begin = "", string end = "")
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