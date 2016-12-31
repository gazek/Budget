using Budget.API.Models;
using Budget.API.Services;
using Budget.API.Services.OFXClient;
using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Http;

namespace Budget.API.Controllers
{
    [Authorize]
    public class AccountController : ControllerBase
    {
        public AccountController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        // create account
        [Route("Account", Name = "CreateAccount")]
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
        [Route("Account/{id}", Name = "GetAccountById")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult Get(int id)
        {
            AccountModel entity = _dbContext.Accounts.Find(id);

            if (entity == null)
            {
                return NotFound();
            }

            if (entity.UserId != User.Identity.GetUserId())
            {
                return Unauthorized();
            }

            return Ok(ModelMapper.EntityToView(entity));
        }

        // get all accounts owned by user
        [Route("Account", Name = "GetAllAccounts")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult Get()
        {
            string userId = User.Identity.GetUserId();
            List<AccountModel> entities = _dbContext.Accounts
                .Where(x => x.UserId == userId)
                .ToList();

            List<AccountViewModel> result = entities.Select(x => ModelMapper.EntityToView(x)).ToList();

            return Ok(result);
        }

        // update account properties when owned by user
        [Route("Account/{id}", Name = "UpdateAccount")]
        [HttpPut]
        [Authorize]
        public IHttpActionResult Update(int id, AccountBindingModel model)
        {
            // look for record
            AccountModel record = _dbContext.Accounts.Find(id);

            // check if record exists
            if (record == null)
            {
                return NotFound();
            }

            // check if user owns record
            if (record.UserId != User.Identity.GetUserId())
            {
                return Unauthorized();
            }

            // verify model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // make updates
            foreach (string key in model.GetType().GetProperties().Select(x => x.Name))
            {
                record.GetType().GetProperty(key).SetValue(record, model.GetType().GetProperty(key).GetValue(model));
            }

            try
            {
                // commit changes
                int result = _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return GetErrorResult(ex);
            }

            // return updated record
            return Ok();
        }

        // TODO:
        //   Add routes to query by: amount
        //                           payee
        //                           category
        //                           subcategory
        //  Should this API be under account or transactions or both?
        
    }
}
