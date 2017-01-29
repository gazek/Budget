using Budget.API.Models;
using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Web.Http;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api")]
    public class PayeeController : ControllerBase<PayeeModel>
    {
        public PayeeController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        // create payee
        [Route("Payee", Name = "CreatePayee")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(PayeeBindingModel model)
        {
            // make sure it is not unassigned
            VerifyName(model.Name ?? "");
            return Create<PayeeBindingModel, IPrincipal>(model, User);
        }

        // get payee by ID
        [Route("Payee/{id}", Name = "GetPayeeById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            return base.Get(id);
        }

        // get all payees owned by user
        [Route("Payee", Name = "GetPayeeByUserId")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult GetAll()
        {
            string userId = User.Identity.GetUserId();

            // filters
            var filter = new List<Expression<Func<PayeeModel, bool>>>();
            filter.Add(p => p.UserId == userId);

            return base.GetAll<PayeeModel, string>(filter, null, p => p.Name);
        }

        // uodate payee
        [Route("Payee/{id}", Name = "UpdatePayee")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult Update(int id, PayeeBindingModel model)
        {
            // make sure new name is not unassigned
            VerifyName(model?.Name ?? "");
            // make sure record being updated is not unassigned
            VerifyName(_dbContext.Payees.Find(id)?.Name ?? "");

            return base.Update(id, model);
        }

        // delete payee
        [Route("Payee/{id}", Name = "DeletePayee")]
        [HttpDelete]
        [Authorize]
        public IHttpActionResult Delete(int id, PayeeBindingModel model)
        {
            // make sure record being deleted is not unassigned
            VerifyName(_dbContext.Payees.Find(id)?.Name ?? "");

            return Delete(id);
        }

        private void VerifyName(string name)
        {
            // make sure it is not unassigned
            if (name.ToLower().Contains("unassigned"))
            {
                SetErrorResponse(BadRequest("Unassigned payee may not be added, modified or deleted"));
            }
        }
    }
}