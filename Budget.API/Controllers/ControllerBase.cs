using Budget.API.Services;
using Budget.API.Services.OFXClient;
using Budget.DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace Budget.API.Controllers
{
    public class ControllerBase<T> : ApiController where T : class
    {
        protected IApplicationDbContext _dbContext;
        protected ApplicationUserManager _userManager;
        protected bool _requestIsOk;
        protected bool _isAuthorized;
        protected int _commitResult;
        protected dynamic _record;
        protected IHttpActionResult _errorResponse;
        public IOfxClient OfxClient { get; set; }

        public ControllerBase(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _requestIsOk = true;
            _isAuthorized = false;
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

        #region CRUD Methods

        #region Get by ID
        public virtual IHttpActionResult Get(int id)
        {
            return Get<T>(id);
        }

        public virtual IHttpActionResult Get<Tr>(int id) where Tr : class
        {
            Expression<Func<Tr, bool>> idFunc = x => (int)x.GetType().GetProperty("Id").GetValue(x) == id;
            List<Expression<Func<Tr, bool>>> where = new List<Expression<Func<Tr, bool>>>() { idFunc };
            var response =  Get<Tr>(where);
            if (response.GetType() == typeof(OkNegotiatedContentResult<ICollection<object>>))
            {
                OkNegotiatedContentResult<ICollection<object>> responseTyped = response as OkNegotiatedContentResult<ICollection<object>>;
                var result = responseTyped.Content.FirstOrDefault();
                if (result == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(result);
                }
            }
            else
            {
                return response;
            }
        }

        public virtual IHttpActionResult Get<Tr>(ICollection<Expression<Func<Tr, bool>>> where, ICollection<Expression<Func<Tr, object>>> include = null) where Tr : class
        {
            // look for record
            // check record exists
            // verify user is authorized to access record
            GetRecordAndIsAuthorized<Tr>(where, include);
            
            // return response
            if (_requestIsOk)
            {
                return Ok(ModelMapper.EntityToView<T>(_record));
            }
            else
            {
                return _errorResponse;
            }
        }
        #endregion

        #region Get all
        public virtual IHttpActionResult GetAll()
        {
            return GetAll<T>();
        }

        public virtual IHttpActionResult GetAll<TEntity>() where TEntity : class
        {
            string userId = User.Identity.GetUserId();
            List<Expression<Func<TEntity, bool>>> funcs = new List<Expression<Func<TEntity, bool>>>()
            {
                x => (string)x.GetType().GetProperty("UserId").GetValue(x) == userId
            };
            return GetAll<TEntity>(funcs);
        }

        public virtual IHttpActionResult GetAll<TEntity>(ICollection<Expression<Func<TEntity, bool>>> where,
            ICollection<string> include = null,
            Expression<Func<TEntity, object>> orderby = null,
            bool orderByOrderByDescending = false) where TEntity : class
        {
            // get dbset
            DbSet<TEntity> dbSet = GetDbSet<TEntity>();
            // variable to store intermidiate query
            IQueryable<TEntity> queriable = dbSet;
            // filter
            foreach (Expression<Func<TEntity, bool>> e in where)
            {
                queriable = queriable.Where(e);
            }
            // include related entities
            if (include != null)
            {
                foreach (string i in include)
                {
                    queriable = queriable.Include(i);
                }
            }
            // order results
            if (orderby != null)
            {
                if (orderByOrderByDescending)
                {
                    queriable = queriable.OrderByDescending(orderby);
                }
                else
                {
                    queriable = queriable.OrderBy(orderby);
                }
            }
            // convert to list
            List<TEntity> entities = queriable.ToList();
            // map to view model
            var result = entities.Select(x => ModelMapper.EntityToView<TEntity>(x)).ToList();
            // return result
            return Ok(result);
        }
        #endregion

        #region Update
        public virtual IHttpActionResult Update<Tb>(int id, Tb model)
        {
            // look for record
            // check record exists
            // verify user is authorized to access record
            GetRecordAndIsAuthorized(id);

            // verify model is valid
            VerifyModel();

            // make updates
            UpdateRecord<Tb>(_record, model);

            // commit changes and check result
            CommitChanges();

            // return response
            if (_requestIsOk)
            {
                return Ok();
            }
            else
            {
                return _errorResponse;
            }
        }
        #endregion

        #region Create
        public virtual IHttpActionResult Create<Tb, Tv, TPrincipal>(Tb model, TPrincipal principal, Func<int, string> locationFunc = null)
            where Tb : class
            where TPrincipal : class
        {
            return Create<Tb, object, TPrincipal>(model, 0, principal, locationFunc);
        }

        public virtual IHttpActionResult Create<Tb, TAuth>(Tb model, int authId, Func<int, string> locationFunc = null)
            where Tb : class
            where TAuth : class
        {
            return Create<Tb, TAuth, TAuth> (model, authId, null, locationFunc);
        }

        private IHttpActionResult Create<Tb, TAuth, TPrincipal>(Tb model, int authId, TPrincipal principal, Func<int, string> locationFunc = null)
            where Tb : class
            where TAuth : class
            where TPrincipal : class
        {
            // look for principal record
            // check if record exists
            // verify user is authorized create dependent entity
            if (authId > 0)
            {
                GetRecordAndIsAuthorized<TAuth>(authId);
                principal = _record;
            }

            // verify model is valid
            VerifyModel();

            // create entity
            T newEntity = CreateEntity<Tb, TPrincipal>(model, principal);

            // add to context
            T newRecord = AddEntityToContext(newEntity);

            // commit changes
            CommitChanges();

            // return result
            if (_requestIsOk)
            {
                // convert new record to view model
                var result = ModelMapper.EntityToView<T>(newRecord);

                // set location
                string location = "";
                if (locationFunc != null)
                {
                    location = locationFunc(result.Id);
                }
                // return result
                return Created(location, result);
            }
            else
            {
                return _errorResponse;
            }
        }
        #endregion

        #region Delete

        #endregion

        #endregion

        #region Get Record
        protected void GetRecord(int id)
        {
            GetRecord<T>(id);
        }

        protected void GetRecord<Tr>(int id) where Tr : class
        {
            Expression<Func<Tr, bool>> idFunc = x => (int)x.GetType().GetProperty("Id").GetValue(x) == id;
            List<Expression<Func<Tr, bool>>> where = new List<Expression<Func<Tr, bool>>>() { idFunc };
            GetRecord<Tr>(where);
        }

        protected void GetRecord<Tr>(ICollection<Expression<Func<Tr, bool>>> where, ICollection<Expression<Func<Tr, object>>> include = null) where Tr : class
        {
            // get DbSet to query
            DbSet<Tr> dbset = GetDbSet<Tr>();
            // IQueryable to hold intermediate query result
            IQueryable<Tr> qRecords = dbset;

            // apply filtering
            foreach (Expression<Func<Tr, bool>> e in where)
            {
                qRecords = qRecords.Where(e);
            }

            // load related entities
            if (include != null)
            {
                foreach (Expression<Func<Tr, object>> i in include)
                {
                    qRecords = qRecords.Include<Tr, object>(i);
                }
            }

            // reduce to single element and store
            _record = qRecords.FirstOrDefault() as Tr;
        }
        #endregion

        #region GetRecordAndIsAuthorized
        protected void GetRecordAndIsAuthorized(int id)
        {
            GetRecordAndIsAuthorized<T>(id);
        }

        protected void GetRecordAndIsAuthorized<Tr>(int id) where Tr : class
        {
            Expression<Func<Tr, bool>> idFunc = x => (int)x.GetType().GetProperty("Id").GetValue(x) == id;
            List<Expression<Func<Tr, bool>>> where = new List<Expression<Func<Tr, bool>>>() { idFunc };
            GetRecordAndIsAuthorized<Tr>(where);
        }

        protected void GetRecordAndIsAuthorized<Tr>(ICollection<Expression<Func<Tr, bool>>> where, ICollection<Expression<Func<Tr, object>>> include = null) where Tr : class
        {
            GetRecord<Tr>(where, include);
            RecordExists();
            IsAuthorized();
        }
        #endregion

        protected void UpdateRecord<Tb>(T existingRecord, Tb update)
        {
            if (existingRecord == null)
            {
                return;
            }

            foreach (string key in update.GetType().GetProperties().Select(x => x.Name))
            {
                object theType = existingRecord.GetType().GetProperty(key).GetValue(existingRecord);
                if (!(theType is ICollection))
                {
                    existingRecord.GetType().GetProperty(key).SetValue(existingRecord, update.GetType().GetProperty(key).GetValue(update));
                }
            }
        }

        protected T CreateEntity<Tb>(Tb model)
        {
            return ModelMapper.BindingToEntity<Tb>(model);
        }

        protected T CreateEntity<Tb, TPrincipal>(Tb model, TPrincipal principal)
        {
            if (principal == null)
            {
                return ModelMapper.BindingToEntity<Tb>(model);
            }
            else
            {
                return ModelMapper.BindingToEntity<Tb, TPrincipal>(model, principal);
            }
        }

        protected T AddEntityToContext(T entity)
        {
            DbSet<T> dbset = GetDbSet<T>();
            return dbset.Add(entity);
        }

        private DbSet<TEntity> GetDbSet<TEntity>() where TEntity : class
        {
            var properties = _dbContext.GetType().GetProperties();
            foreach (var p in properties)
            {
                if (p.PropertyType == typeof(DbSet<TEntity>))
                {
                    return p.GetValue(_dbContext, null) as DbSet<TEntity>;
                }
            }
            return null;
        }

        #region Record Exists
        protected void RecordExists()
        {
            RecordExists(_record);
        }
        
        protected void RecordExists(object record)
        {
            if(record == null)
            {
                _requestIsOk = false;
                SetErrorResponse(NotFound());
            }
        }
        #endregion

        #region Is Authorized
        protected void IsAuthorized()
        {
            IsAuthorized(_record);
        }
        
        protected void IsAuthorized(object record)
        {
            if (record == null)
            {
                return;
            }
            
            string userId = ModelMapper.GetUserId<T>(record, _dbContext);
            if (userId == User.Identity.GetUserId())
            {
                _isAuthorized = true;
            }
            else
            {
                _isAuthorized = false;
                _requestIsOk = false;
                SetErrorResponse(Unauthorized());
            }
        }
        #endregion

        protected void CommitChanges()
        {
            try
            {
                // commit changes
                _commitResult = _dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                SetErrorResponse(GetErrorResult(ex));
            }
        }

        protected void VerifyModel()
        {
            if (!ModelState.IsValid)
            {
                SetErrorResponse(BadRequest(ModelState));
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

        private void SetErrorResponse(IHttpActionResult response)
        {
            // set in error flag
            _requestIsOk = false;
            // set error response, if not already set
            if (_errorResponse == null)
            {
                _errorResponse = response;
            }
        }
    }
}