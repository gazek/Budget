using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/FinancialInstitution")]
    public class FinancialInstitutionsController : ApiController
    {
        // POST add new FI
        // PUT Update existing FI (excluding login credentials and only if owned by requesting user)
        // PUT Add or change FI login credentials (only if owned by requesting user)
        // DELETE Delete FI
        // GET Get all FIs owned by a specific user
        // GET Get FI by ID (only if owned by requesting user)
    }
}
