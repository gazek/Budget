using Budget.API.Models;
using Budget.API.Services;
using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
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

        [Route("", Name = "CreateCategory")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(CategoryBindingModel model)
        {
            // verify cat and subcat names do not contain 'uncategorized'
            // verfiy cat and subcat ID are not used in transactions
            //    if they are either return BadRequest of set transactions to uncat

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CategoryModel entity = ModelMapper.BindingToEntity(model, User);

            CategoryModel record = _dbContext.Categories.Add(entity);
            try
            {
                int result = _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return GetErrorResult(ex);
            }

            CategoryViewModel viewmodel = ModelMapper.EntityToView(record);

            return Created(Url.Link("GetCategoryById", new { id = record.Id }), viewmodel);
        }

        [Route("{id}", Name = "UpdateCategory")]
        [HttpPut]
        public IHttpActionResult Update(int id, CategoryBindingModel model)
        {
            // verify cat and subcat names do not contain 'uncategorized'
            // check if any subcats are being deleted
            //    if subcat deletion and deleted subcat is used in transaction
            //    either give badrequest response to set transactions subcat to use uncat subcatId

            // look for record
            CategoryModel record = _dbContext.Categories.Where(c => c.Id == id).FirstOrDefault();

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
            CategoryModel entity = ModelMapper.BindingToEntity(model, User);
            record.Name = entity.Name;
            // make a sub category controller to do CRUD and usage checks
            //record.SubCategories = entity.SubCategories;

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

        [Route("{id}", Name = "GetCategoryById")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult Get(int id)
        {
            CategoryModel entity = _dbContext.Categories
                .Where(c => c.Id == id)
                .Include(c => c.SubCategories)
                .FirstOrDefault();

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

        // get all categories owned by user
        [Route("", Name = "GetAllCategories")]
        [HttpGet]
        [Authorize]
        public override IHttpActionResult GetAll()
        {
            string userId = User.Identity.GetUserId();
            List<CategoryModel> entities = _dbContext.Categories
                .Where(x => x.UserId == userId)
                .Include(c => c.SubCategories)
                .ToList();

            List<CategoryViewModel> result = entities.Select(x => ModelMapper.EntityToView(x)).ToList();

            return Ok(result);
        }
    }
}