using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Budget.API.Models;
using Budget.DAL;
using Microsoft.AspNet.Identity.Owin;
using Budget.DAL.Models;
using Budget.API.Services;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/FinancialInstitution")]
    public class FinancialInstitutionsController : ApiController
    {
        private ApplicationDbContext _dbContext;
        private ApplicationUserManager _userManager;

        public FinancialInstitutionsController()
        {
            _dbContext = new ApplicationDbContext();
        }

        public FinancialInstitutionsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public FinancialInstitutionsController(ApplicationUserManager userManager)
        {
            _dbContext = new ApplicationDbContext();
            UserManager = userManager;
        }

        public FinancialInstitutionsController(ApplicationDbContext dbContext, ApplicationUserManager userManager)
        {
            _dbContext = dbContext;
            UserManager = userManager;
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

            var ent = ModelMapper.BindingToEntity(model, User);
            var record = _dbContext.FinancialInstitutions.Add(ent);
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

            SqlException exception = (SqlException)ex.InnerException.InnerException;
            if (exception.Number == 2601)
            {
                return BadRequest(errors[exception.Number]);
            }

            return BadRequest();
        }
    }
}
