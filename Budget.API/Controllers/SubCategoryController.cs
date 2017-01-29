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
    public class SubCategoryController : ControllerBase<SubCategoryModel>
    {
        public SubCategoryController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }

        [Route("api/Category/{catId}/SubCategory/{id}", Name = "DeleteSubCategory")]
        [HttpDelete]
        [Authorize]
        public IHttpActionResult Delete(int id, int catId)
        {
            // make sure record being deleted is not uncategorized
            VerifyName(_dbContext.Categories.Find(catId)?.Name ?? "");

            return Delete(id);
        }

        [Route("api/Category/{catId}/SubCategory", Name = "CreateSubCategory")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(SubCategoryBindingModel model, int catId)
        {
            // make sure record being created is not uncategorized
            VerifyName(_dbContext.Categories.Find(catId)?.Name ?? "");
            // Add CategoryId to model
            model.CategoryId = catId;
            // call base create
            return Create<SubCategoryBindingModel, CategoryModel>(model, catId);
        }

        [Route("api/Category/{catId}/SubCategory/{id}", Name = "UpdateSubCategory")]
        [HttpPut]
        public IHttpActionResult Update(SubCategoryBindingModel model, int id, int catId)
        {
            // make sure record being updated is not uncategorized
            VerifyName(_dbContext.Categories.Find(catId)?.Name ?? "");
            // Add CategoryId to model
            model.CategoryId = catId;
            return Update(id, model);
        }

        [Route("api/Category/{catId}/SubCategory/{id}", Name = "GetSubCategoryById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            return base.Get(id);
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