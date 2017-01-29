using Budget.API.Models;
using Budget.DAL;
using Budget.DAL.Models;
using System;
using System.Web.Http;

namespace Budget.API.Controllers
{
    public class PayeeImportNameController : ControllerBase<PayeeImportNameModel>
    {
        public PayeeImportNameController(IApplicationDbContext dbContext) : base(dbContext)
        {

        }

        // create payee import name
        [Route("api/Payee/{pId}/ImportName", Name = "CreatePayeeImportName")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(PayeeImportNameBindingModel model, int pId)
        {
            model.PayeeId = pId;
            Func<int, string> location = x => Url.Link("GetPayeeImportNameById", new { id = x });
            return base.Create<PayeeImportNameBindingModel, PayeeModel>(model, pId);
        }

        // get payee import name by ID
        [Route("api/Payee/{pId}/ImportName/{id}", Name = "GetPayeeImportNameById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            return base.Get(id);
        }

        // update payee import name properties when owned by user
        [Route("api/Payee/{pId}/ImportName/{id}", Name = "UpdatePayeeImportName")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult Update(int id, int pId, PayeeImportNameBindingModel model)
        {
            model.PayeeId = pId;
            return base.Update(id, model);
        }

        // delete payee import name
        [Route("api/Payee/{pId}/ImportName/{id}", Name = "DeletePayeeImportName")]
        [HttpDelete]
        [Authorize]
        public override IHttpActionResult Delete(int id)
        {
            return base.Delete(id);
        }
    }
}