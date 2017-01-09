using Budget.API.Models;
using Budget.API.Services;
using Budget.DAL;
using Budget.DAL.Models;
using System.Data.Entity.Infrastructure;
using System.Web.Http;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ControllerBase<AccountModel>
    {
        public AccountController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        // create account
        [Route("", Name = "CreateAccount")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(AccountBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            AccountModel entity = ModelMapper.BindingToEntity(model, User);

            AccountModel record = _dbContext.Accounts.Add(entity);
            try
            {
                int result = _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return GetErrorResult(ex);
            }

            AccountViewModel viewmodel = ModelMapper.EntityToView(record);

            return Created(Url.Link("GetAccountById", new { id = record.Id }), viewmodel);
        }

        // get account by ID
        [Route("{id}", Name = "GetAccountById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            return base.Get(id);
        }

        // get all accounts owned by user
        [Route("", Name = "GetAllAccounts")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult GetAll()
        {
            return base.GetAll<AccountModel>();
        }

        // update account properties when owned by user
        [Route("{id}", Name = "UpdateAccount")]
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
