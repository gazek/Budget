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
        public override IHttpActionResult Delete(int id)
        {
            // make sure record being deleted is not uncategorized
            VerifyName(_dbContext.Categories.Find(id)?.Name ?? "");

            return base.Delete(id);
        }

        [Route("", Name = "CreateCategory")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(CategoryBindingModel model)
        {
            // make sure it is not uncategorized
            VerifyName(model.Name ?? "");
            return Create<CategoryBindingModel, IPrincipal>(model, User);
        }

        [Route("{id}", Name = "UpdateCategory")]
        [HttpPut]
        public IHttpActionResult Update(int id, CategoryBindingModel model)
        {
            // make sure new name is not uncategorized
            VerifyName(model?.Name ?? "");
            // make sure record being updated is not uncategorized
            VerifyName(_dbContext.Categories.Find(id).Name ?? "");

            // filters
            List<Expression<Func<CategoryModel, bool>>> filters = new List<Expression<Func<CategoryModel, bool>>>();
            filters.Add(c => c.Id == id);

            // include related entities
            List<Expression<Func<CategoryModel, object>>> include = new List<Expression<Func<CategoryModel, object>>>();
            include.Add(c => c.SubCategories);

            return Update<CategoryBindingModel, CategoryModel, SubCategoryViewModel>(model, filters, include);
        }

        [Route("{id}", Name = "GetCategoryById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            // filters
            List<Expression<Func<CategoryModel, bool>>> filters = new List<Expression<Func<CategoryModel, bool>>>();
            filters.Add(c => c.Id == id);

            // include related entities
            List<Expression<Func<CategoryModel, object>>> include = new List<Expression<Func<CategoryModel, object>>>();
            include.Add(c => c.SubCategories);

            return base.Get<CategoryModel>(filters, include);
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

        private void VerifyName(string name)
        {
            // make sure it is not uncategorized
            if (name.ToLower().Contains("uncategorized"))
            {
                SetErrorResponse(BadRequest("Uncategorized category may not be added, modified or deleted"));

            }
        }
    }
}