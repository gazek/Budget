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
using System.Reflection;
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
            List<Expression<Func<Tr, bool>>> filter = new List<Expression<Func<Tr, bool>>>();
            filter.Add(BuildFilterExpression<Tr, int>(typeof(Tr).GetProperty("Id"), id));
            return Get<Tr>(filter);
        }
        /*
         * Tr - type of record requested and type of authorization record
         * where - lambda expressions which filter DbSet to the requested/authorization record
         * include - lambda expressions used to load related entities
         */
        public virtual IHttpActionResult Get<Tr>(ICollection<Expression<Func<Tr, bool>>> where, ICollection<Expression<Func<Tr, object>>> include = null) where Tr : class
        {
            // look for record
            // check record exists
            // verify user is authorized to access record
            GetRecordAndIsAuthorized<Tr>(where, include);
            
            // return response
            return _requestIsOk ? Ok(ModelMapper.EntityToView(_record, _dbContext)) : _errorResponse;
        }
        #endregion

        #region Get all
        public virtual IHttpActionResult GetAll()
        {
            return GetAll<T>();
        }

        public virtual IHttpActionResult GetAll<Tr>() where Tr : class
        {
            string userId = User.Identity.GetUserId();
            List<Expression<Func<Tr, bool>>> filter = new List<Expression<Func<Tr, bool>>>();
            filter.Add(BuildFilterExpression<Tr, string>(typeof(Tr).GetProperty("UserId"), userId));
            return GetAll<Tr, object>(filter);
        }
        /*
         * Tr - type of record requested
         * TSort - Sort by type
         * where - filtering lambda expressions to create the return set from DbSet
         * include - lambda expressions used to load related entities
         * orderby - lambda expression to sort the return set
         * orderByOrderByDescending - set ascending/descending sort order
         */
        public virtual IHttpActionResult GetAll<Tr, TSort>(ICollection<Expression<Func<Tr, bool>>> where,
            ICollection<string> include = null,
            Expression<Func<Tr, TSort>> orderby = null,
            bool orderByOrderByDescending = false) where Tr : class
        {
            // get dbset
            DbSet<Tr> dbSet = GetDbSet<Tr>();
            // variable to store intermidiate query
            IQueryable<Tr> queriable = dbSet;
            // filter
            foreach (Expression<Func<Tr, bool>> e in where)
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
            var entities = queriable.ToList();
            // map to view model
            var result = entities.Select(x => ModelMapper.EntityToView(x, _dbContext)).ToList();
            // return result
            return Ok(result);
        }
        #endregion

        #region Update
        /*
         * Tb - type of binding model
         */
        public virtual IHttpActionResult Update<Tb>(int id, Tb model)
        {
            // look for record
            // check record exists
            // verify user is authorized to access record
            GetRecordAndIsAuthorized(id);

            return _Update<Tb>(model);
        }

        public virtual IHttpActionResult Update<Tb,Te>(Tb model, ICollection<Expression<Func<Te, bool>>> where, ICollection<Expression<Func<Te, object>>> include = null) where Te : class
        {
            // look for record
            // check record exists
            // verify user is authorized to access record
            // ** IMPORTANT **
            //   if collections of related entities are included using the include argument,
            //   the full collection must be included in the update model, any items in the DB
            //   that are not included in the update binding model will be deleted aspart of the update
            GetRecordAndIsAuthorized<Te>(where, include);

            return _Update<Tb>(model);
        }

        public IHttpActionResult _Update<Tb>(Tb model)
        {
            // verify model is valid
            VerifyModel();

            // map to entity
            T entity = ModelMapper.BindingToEntity(model, _dbContext);

            // make updates
            UpdateRecord<T, T>(_record, entity);

            // commit changes and check result
            CommitChanges();

            // return response
            return _requestIsOk ? Ok() : _errorResponse;
        }
        #endregion

        #region Create
        public virtual IHttpActionResult Create<Tb, TPrincipal>(Tb model, TPrincipal principal, Func<int, string> locationFunc = null)
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
        /*
         * Tb - type of binding model
         * TAuth - type of record used to verify authorization to create
         * TPrincipal - type of record which 'owns' the type being created
         * 
         * TAuth & TPrincipal are usually the same type
         * 
         * Examples
         *      - Tb = TransactionModel, Tprincipal = AccountModel, TAuth = AccountModel
         *      - Tb = CategoryModel, TAuth
         */
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
                var result = ModelMapper.EntityToView(newRecord, _dbContext);

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
        public virtual IHttpActionResult Delete(int id)
        {
            // look for record
            // check record exists
            // verify user is authorized to access record
            GetRecordAndIsAuthorized(id);

            // delete record if not referenced in other tables
            DeleteEntityFromContext(id);

            // commit changes and check result
            CommitChanges();

            // return response
            return _requestIsOk ? Ok() : _errorResponse;
        }
        #endregion

        #endregion

        #region Get Record
        protected void GetRecord(int id)
        {
            GetRecord<T>(id);
        }

        protected void GetRecord<Tr>(int id) where Tr : class
        {
            List<Expression<Func<Tr, bool>>> filter = new List<Expression<Func<Tr, bool>>>();
            filter.Add(BuildFilterExpression<Tr, int>(typeof(Tr).GetProperty("Id"), id));
            GetRecord<Tr>(filter);
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
            List<Expression<Func<Tr, bool>>> filter = new List<Expression<Func<Tr, bool>>>();
            filter.Add(BuildFilterExpression<Tr, int>(typeof(Tr).GetProperty("Id"), id));
            GetRecordAndIsAuthorized<Tr>(filter);
        }

        protected void GetRecordAndIsAuthorized<Tr>(ICollection<Expression<Func<Tr, bool>>> where, ICollection<Expression<Func<Tr, object>>> include = null) where Tr : class
        {
            GetRecord<Tr>(where, include);
            RecordExists();
            IsAuthorized();
        }
        #endregion

        protected void UpdateRecord<Tex, Tup>(Tex existingRecord, Tup update)
        {
            if (!_requestIsOk || existingRecord == null)
            {
                return;
            }

            foreach (string key in update.GetType().GetProperties().Select(x => x.Name))
            {
                if (key != "Id")
                {
                    var value = update.GetType().GetProperty(key).GetValue(update);
                    if (value is ICollection && !(value is Byte[]))
                    {
                        Type type = value.GetType().GetGenericArguments().Single();
                        var prop = _dbContext.GetType()
                            .GetProperties()
                            .Where(x => x.PropertyType.FullName.Contains(type.Name))
                            .FirstOrDefault();
                        dynamic dbSet = prop.GetValue(_dbContext);
                        
                        // delete all records from context which are not in the collection
                        var existingCollection = (ICollection)existingRecord.GetType().GetProperty(key).GetValue(existingRecord);
                        List<int> idsToRemove = new List<int>();
                        foreach (var x in existingCollection)
                        {
                            int id = (int)x.GetType().GetProperty("Id").GetValue(x);
                            Predicate <object> myPred = i => (int)i.GetType().GetProperty("Id").GetValue(i) == id;
                            var fromUpdate = value.GetType().GetMethod("Find").Invoke(value, new Object[] { myPred });

                            if (fromUpdate == null)
                            {
                                idsToRemove.Add(id);
                            }
                        }

                        foreach (int i in idsToRemove)
                        {
                            dbSet.Remove(dbSet.Find(i));
                        }

                        foreach (var x in (ICollection) value)
                        {
                            int id = (int)x.GetType().GetProperty("Id").GetValue(x);
                            if ( id > 0)
                            {
                                var existing = dbSet.Find(id);
                                this.GetType()
                                    .GetMethod("UpdateRecord", BindingFlags.Instance | BindingFlags.NonPublic)
                                    .MakeGenericMethod(type, type)
                                    .Invoke(this, new object[] { existing, x });
                            }
                            else
                            {
                                //dbSet.Add(Convert.ChangeType(x, type));
                                //AddEntityToContext(T entity)
                                this.GetType()
                                    .GetMethod("AddEntityToContext", BindingFlags.Instance | BindingFlags.NonPublic)
                                    .MakeGenericMethod(type)
                                    .Invoke(this, new object[] { x });
                            }
                        }
                    }
                    else
                    {
                        existingRecord.GetType().GetProperty(key).SetValue(existingRecord, value);
                    }
                }
            }
        }

        protected T CreateEntity<Tb>(Tb model) where Tb : class
        {
            return ModelMapper.BindingToEntity(model);
        }

        protected T CreateEntity<Tb, TPrincipal>(Tb model, TPrincipal principal) where Tb : class
        {
            if (principal == null)
            {
                return ModelMapper.BindingToEntity(model);
            }
            else
            {
                return ModelMapper.BindingToEntity(model, principal);
            }
        }

        protected Te AddEntityToContext<Te>(Te entity) where Te : class
        {
            if (_requestIsOk)
            {
                DbSet<Te> dbset = GetDbSet<Te>();
                return dbset.Add(entity);
            }
            return null;
        }

        protected void DeleteEntityFromContext(int id)
        {
            if (_requestIsOk)
            {
                DbSet<T> dbset = GetDbSet<T>();
                var entity = dbset.Find(id);
                dbset.Remove(entity);
            }
        }

        private DbSet<Tr> GetDbSet<Tr>() where Tr : class
        {
            var properties = _dbContext.GetType().GetProperties();
            foreach (var p in properties)
            {
                if (p.PropertyType == typeof(DbSet<Tr>))
                {
                    return p.GetValue(_dbContext, null) as DbSet<Tr>;
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
            
            string userId = ModelMapper.GetUserId(record, _dbContext);
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
            if (_requestIsOk)
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
                { 2601, "Operation failed because the record already exists" },
                { 547, "Delete operation failed because the record is referenced as a foreign key" }
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
                    throw new NotImplementedException();
                }
                return BadRequest(errors[sqlEx.Number]);
            }

            return BadRequest(exception.Message);
        }

        protected void ParseDateRange(string begin, string end, out DateTime beginDate, out DateTime endDate)
        {

            DateTime defaultStartDate = DateTime.Parse("01-01-2000");

            // parse begin date
            if (begin == "")
            {
                beginDate = defaultStartDate;
            }
            else
            {
                DateTime.TryParse(begin, out beginDate);
            }

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
                SetErrorResponse(BadRequest("Invalid begin date"));
            }

            if (endDate == DateTime.MinValue && beginDate != DateTime.MinValue)
            {
                SetErrorResponse(BadRequest("Invalid end date"));
            }

            if (endDate == DateTime.MinValue && beginDate == DateTime.MinValue)
            {
                SetErrorResponse(BadRequest("Invalid begin and end dates"));
            }

            if (endDate < beginDate)
            {
                SetErrorResponse(BadRequest("End date must be later than begin date"));
            }
        }

        protected void SetErrorResponse(IHttpActionResult response)
        {
            // set in error flag
            _requestIsOk = false;
            // set error response, if not already set
            if (_errorResponse == null)
            {
                _errorResponse = response;
            }
        }

        private Expression<Func<TItem, bool>> BuildFilterExpression<TItem, TValue>(PropertyInfo property, TValue value)
        {
            var param = Expression.Parameter(typeof(TItem));
            var body = Expression.Equal(Expression.Property(param, property),
                Expression.Constant(value));
            return Expression.Lambda<Func<TItem, bool>>(body, param);
        }
        
    }
}