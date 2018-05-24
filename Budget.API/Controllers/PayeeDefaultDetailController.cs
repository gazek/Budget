using Budget.API.Models;
using Budget.DAL;
using Budget.DAL.Models;
using System;
using System.Web.Http;

namespace Budget.API.Controllers
{
    public class PayeeDefaultDetailController : ControllerBase<PayeeDefaultDetailModel>
    {
        public PayeeDefaultDetailController(IApplicationDbContext dbContext) : base(dbContext)
        {

        }

        // create payee default detail
        [Route("api/Payee/{pId}/DefaultDetail", Name = "CreatePayeeDefaultDetail")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(PayeeDefaultDetailBindingModel model, int pId)
        {
            model.PayeeId = pId;
            Func<int, string> location = x => Url.Link("GetPayeeDefaultDetailById", new { id = x });
            return base.Create<PayeeDefaultDetailBindingModel, PayeeModel>(model, pId);
        }

        // get payee default detail by ID
        [Route("api/Payee/{pId}/DefaultDetail/{id}", Name = "GetPayeeDefaultDetailById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            return base.Get(id);
        }

        // update payee default detail properties when owned by user
        [Route("api/Payee/{pId}/DefaultDetail/{id}", Name = "UpdatePayeeDefaultDetail")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult Update(int id, int pId, PayeeDefaultDetailBindingModel model)
        {
            model.PayeeId = pId;
            return base.Update<PayeeDefaultDetailBindingModel, PayeeDefaultDetailViewModel>(id, model);
        }

        // delete payee default detail
        [Route("api/Payee/{pId}/DefaultDetail/{id}", Name = "DeletePayeeDefaultDetail")]
        [HttpDelete]
        [Authorize]
        public override IHttpActionResult Delete(int id)
        {
            return base.Delete(id);
        }
    }
}