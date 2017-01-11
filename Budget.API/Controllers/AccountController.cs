using Budget.API.Models;
using Budget.API.Services;
using Budget.DAL;
using Budget.DAL.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
        [Route("Account/", Name = "GetAllAccounts")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult GetAll()
        {
            return base.GetAll<AccountModel>();
        }

        // update account properties when owned by user
        [Route("Account/{id}", Name = "UpdateAccount")]
        [HttpPut]
        [Authorize]
        public override IHttpActionResult Update<AccountBindingModel>(int id, AccountBindingModel model)
        {
            return base.Update(id, model);
        }

        // TODO:
        //   Add routes to query by: amount
        //                           payee
        //                           category
        //                           subcategory
        //  Should this API be under account or transactions or both?
        
    }
}
