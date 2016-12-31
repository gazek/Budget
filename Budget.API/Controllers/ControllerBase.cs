using Budget.API.Services.OFXClient;
using Budget.DAL;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Budget.API.Controllers
{
    public class ControllerBase : ApiController
    {
        protected IApplicationDbContext _dbContext;
        protected ApplicationUserManager _userManager;
        public IOfxClient OfxClient { get; set; }

        public ControllerBase(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            OfxClient = new OfxClient();
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

        protected IHttpActionResult GetErrorResult(DbUpdateException ex, int allowEx = -1, IHttpActionResult returnIfAllowed = null)
        {
            var errors = new Dictionary<int, string>
            {
                { 2601, "Operation failed because record already exists" }
            };

            if (ex.InnerException == null)
            {
                return BadRequest(ex.Message);
            }

            var exception = ex.InnerException;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            SqlException sqlEx = (SqlException)exception;
            if (errors.ContainsKey(sqlEx.Number))
            {
                if (sqlEx.Number == allowEx)
                {

                }
                return BadRequest(errors[sqlEx.Number]);
            }

            return BadRequest(exception.Message);
        }

        protected IHttpActionResult ParseDateRange(string begin, string end, out DateTime beginDate, out DateTime endDate)
        {
            // set to full range
            if (begin == "" && end == "")
            {
                beginDate = DateTime.MinValue;
                endDate = DateTime.Today;
                return null;
            }

            // parse begin date
            DateTime.TryParse(begin, out beginDate);

            // parse end date
            if (end == "")
            {
                endDate = DateTime.Today;
            }
            else
            {
                DateTime.TryParse(end, out endDate);
            }

            if (beginDate == DateTime.MinValue && endDate != DateTime.MinValue)
            {
                return BadRequest("Invalid begin date");
            }

            if (endDate == DateTime.MinValue && beginDate != DateTime.MinValue)
            {
                return BadRequest("Invalid end date");
            }

            if (endDate == DateTime.MinValue && beginDate == DateTime.MinValue)
            {
                return BadRequest("Invalid begin and end dates");
            }

            return null;
        }
    }
}