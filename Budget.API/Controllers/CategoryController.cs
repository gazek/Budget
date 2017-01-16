using Budget.API.Models;
using Budget.API.Services;
using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Web.Http;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/Category")]
    public class CategoryController : ControllerBase<CategoryModel>
    {
        public CategoryController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        [Route("{id}", Name = "DeleteCategory")]
        [HttpDelete]
        [Authorize]
        public IHttpActionResult Delete(int id)
        {
            // look for record
            // check record exists
            // verify user is authorized to access record
            GetRecordAndIsAuthorized(id);

            // make sure it is not uncategorized
            if (_record.Name.ToLower().Contains("uncategorized"))
            {
                SetErrorResponse(BadRequest("Uncategorized category may not be modified or deleted"));
            }

            // delete record if not referenced in other tables
            DeleteEntityFromContext(id);

            // commit changes and check result
            CommitChanges();

            // return response
            return _requestIsOk ? Ok() : _errorResponse;
        }

        [Route("", Name = "CreateCategory")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(CategoryBindingModel model)
        {
            return Create<CategoryBindingModel, IPrincipal>(model, User);
        }

        [Route("{id}", Name = "UpdateCategory")]
        [HttpPut]
        public IHttpActionResult Update(int id, CategoryBindingModel model)
        {
            return Update<CategoryBindingModel>(id, model);
        }

        [Route("{id}", Name = "GetCategoryById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            return base.Get(id);
        }

        // get all categories owned by user
        [Route("", Name = "GetAllCategories")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult GetAll()
        {
            // filters
            List<Expression<Func<CategoryModel, bool>>> filters = new List<Expression<Func<CategoryModel, bool>>>();
            string userId = User.Identity.GetUserId();
            filters.Add(c => c.UserId == userId);

            // get related entities
            var include = new List<string>();
            include.Add("SubCategories");

            return GetAll<CategoryModel, string>(filters, include, c => c.Name);
        }
    }
}