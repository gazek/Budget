using Budget.API.Models;
using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api")]
    public class AccountController : ControllerBase<AccountModel>
    {
        public AccountController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        // create account
        [Route("FinancialInstitution/{fiId}/Account", Name = "CreateAccount")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(int fiId, AccountBindingModel model)
        {
            model.FinancialInstitutionId = fiId;
            Func<int, string> location = x => Url.Link("GetAccountById", new { id = x });
            return base.Create<AccountBindingModel, FinancialInstitutionModel>(model, fiId);
        }

        // get account by ID
        [Route("Account/{id}", Name = "GetAccountById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            return base.Get(id);
        }

        // get all accounts owned by user
        [Route("Account", Name = "GetAllAccountsByUserId")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetAllByUser()
        {
            // find all Financial Institutions owned by user
            string userId = User.Identity.GetUserId();
            var fiIds = _dbContext.FinancialInstitutions
                .Where(f => f.UserId == userId)
                .Select(f => f.Id);

            // filters
            List<Expression<Func<AccountModel, bool>>> filters = new List<Expression<Func<AccountModel, bool>>>();
            filters.Add(a => fiIds.Contains(a.FinancialInstitutionId));

            // get related entities
            var include = new List<string>();
            include.Add("Transactions");
            include.Add("BalanceHistory");

            return GetAll<AccountModel, string>(filters, include, a => a.Name);
        }

        // get all accounts by Fi ID
        [Route("FinancialInstitution/{fiId}/Accounts/", Name = "GetAllAccountsByFI")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetAllByFI(int fiId)
        {
            // filters
            List<Expression<Func<AccountModel, bool>>> filters = new List<Expression<Func<AccountModel, bool>>>();
            filters.Add(a => a.FinancialInstitutionId == fiId);

            // get related entities
            var include = new List<string>();
            include.Add("BalanceHistory");

            // verify existence of account
            // and that user is authorized to access it
            GetRecordAndIsAuthorized<FinancialInstitutionModel>(fiId);

            if (!_requestIsOk)
            {
                return _errorResponse;
            }

            return base.GetAll<AccountModel, string>(filters, include, a => a.Name);
        }

        // update account properties when owned by user
        [Route("FinancialInstitution/{fiId}/Account/{id}", Name = "UpdateAccount")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult Update(int id, int fiId, AccountBindingModel model)
        {
            model.FinancialInstitutionId = fiId;
            model.Id = id;
            return base.Update(id, model);
        }

        // delete account
        [Route("Account/{id}", Name = "DeleteAccount")]
        [HttpDelete]
        [Authorize]
        public override IHttpActionResult Delete(int id)
        {
            return base.Delete(id);
        }

        // delete account
        [Route("Account/Types", Name = "GetAccountTypes")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetAccountTypes()
        {
            string[] types = Enum.GetNames(typeof(AccountType));
            return Ok(types);
        }
    }
}
