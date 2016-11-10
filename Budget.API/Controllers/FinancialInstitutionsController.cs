using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Budget.API.Models;
using Budget.DAL;
using Microsoft.AspNet.Identity.Owin;
using Budget.API.Services;
using System.Data.SqlClient;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/FinancialInstitution")]
    public class FinancialInstitutionsController : ApiController
    {
        private IApplicationDbContext _dbContext;
        private ApplicationUserManager _userManager;

        public FinancialInstitutionsController(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
       
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [Route("")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(FinancialInstitutionCreateBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = ModelMapper.BindingToEntity(model, User);

            var record = _dbContext.FinancialInstitutions.Add(entity);
            try
            {
                var result = _dbContext.SaveChanges();
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                return GetErrorResult(ex);
            }

            return Ok(ModelMapper.EntityToView(record));
        }
        
        // POST add new FI
        // PUT Update existing FI (excluding login credentials and only if owned by requesting user)
        // PUT Add or change FI login credentials (only if owned by requesting user)
        // DELETE Delete FI
        // GET Get all FIs owned by a specific user
        // GET Get FI by ID (only if owned by requesting user)

        private IHttpActionResult GetErrorResult(System.Data.Entity.Infrastructure.DbUpdateException ex)
        {
            var errors = new Dictionary<int, string>
            {
                { 2601, "Operation failed because record already exists" }
            };

            var exception = ex.InnerException;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            try
            {
                SqlException sqlEx = (SqlException)exception;
                if (errors.ContainsKey(sqlEx.Number))
                {
                    return BadRequest(errors[sqlEx.Number]);
                }
            }
            catch
            {
                return BadRequest(exception.Message);
            }

            return BadRequest(exception.Message);
        }
    }
}
