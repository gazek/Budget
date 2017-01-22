using Budget.API.Models;
using Budget.DAL;
using Budget.DAL.Models;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using Budget.API.Services;
using Microsoft.AspNet.Identity;

namespace Budget.API.Controllers
{
    public class TransactionDetailController : ControllerBase<TransactionDetailModel>
    {
        public TransactionDetailController(IApplicationDbContext dbContext) : base(dbContext)
        {
        }
        /*
         * Write operations are done on the entire collection of details using a single route
         * 
         * There are no read or write operations for individual details
         *      This is required to simplify the effort needed to keep
         *      the details sum balanced with the transaction amount.
         *      If individual details could be operated upon, the server
         *      would potentially need to add or remove a balancing entry
         *      after every write operation which would make keeping the 
         *      displayed data in sync with the DB difficult.
         * 
         * Details can only be retrieved as a property of a retrieved Transaction 
         * 
         */

        [Route("api/Transaction/{TransId}/Details", Name = "CreateTransactionDetails")]
        [HttpPost]
        [Authorize]
        public IHttpActionResult Create(ICollection<TransactionDetailBindingModel> models, int transId)
        {
            // verify user if authorized to edit this transaction's details
            GetRecordAndIsAuthorized<TransactionModel>(transId);

            // delete transaction's current details
            DeleteCurrentDetailsFromContext();

            // add new transactions to context
            AddNewDetailsToContext(models);

            // verify sum of details equals transaction amount
            // if needed, add uncat detail to balance
            BalanceDetailsSum();

            // commit changes
            CommitChanges();

            // collect new set of details
            ICollection<TransactionDetailModel> details = _record.Details;

            // return new set of details
            return _requestIsOk ? Ok(details.Select(d => ModelMapper.EntityToView(d, _dbContext))) : _errorResponse;
        }

        private void AddNewDetailsToContext(ICollection<TransactionDetailBindingModel> models)
        {
            IEnumerable<TransactionDetailModel> entities = models.Select(d => ModelMapper.BindingToEntity(d, (TransactionModel)_record));
            _dbContext.TransactionDetails.AddRange(entities);
        }

        private void DeleteCurrentDetailsFromContext()
        {
            if (_requestIsOk)
            {
                int transId = _record.Id;
                var details = _dbContext.TransactionDetails.Where(d => d.TransactionId == transId);
                _dbContext.TransactionDetails.RemoveRange(details);
            }
        }

        private void BalanceDetailsSum()
        {
            TransactionDetailsChecker detailsChecker = new TransactionDetailsChecker(_record, _dbContext);
            if (!detailsChecker.AmountIsFullyCategorized)
            {
                _record.Details.Add(detailsChecker.UncategorizedDetail);
            }
        }
    }
}