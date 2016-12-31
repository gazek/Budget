using Budget.API.Services.OFXClient;
using Budget.DAL;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using Microsoft.AspNet.Identity.Owin;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using Budget.API.Models;
using Budget.API.Services;
using System;

namespace Budget.API.Controllers
{
    [Authorize]
    public class BalanceController : ControllerBase
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
            AccountModel account = _dbContext.Accounts.Find(id);

            if (account == null)
            {
                return NotFound();
            }

            if (account.UserId != User.Identity.GetUserId())
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

            List<BalanceModel> balances = _dbContext.Balances
                .Where(b => b.AccountId == account.Id)
                .Where(b => b.AsOfDate >= beginDate)
                .Where(b => b.AsOfDate <= endDate)
                .OrderByDescending(b => b.AsOfDate).ToList();

            List<BalanceViewModel> result = balances?.Select(x => ModelMapper.EntityToView(x)).ToList();

            return Ok(result);
        }
    }
}