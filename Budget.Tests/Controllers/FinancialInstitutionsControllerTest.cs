using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Budget.API.Controllers;
using Budget.DAL;
using Budget.API.Models;
using Budget.API.Services;
using System.Security.Principal;
using System.Collections.Generic;
using System.Security.Claims;
using System.Data.Entity;
using Budget.DAL.Models;
using System.Web.Http.Results;
using System.Web.Http;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Web.Http.Routing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Budget.API.Tests.Fakes;

namespace Budget.API.Tests.Controllers
{
    [TestClass]
    public class FinancialInstitutionsControllerTest
    {

        [TestMethod]
        public void FICreateWithValidModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);
            var urlHelperMock = new Moq.Mock<UrlHelper>();
            urlHelperMock.Setup(x => x.Link(Moq.It.IsAny<string>(), Moq.It.IsAny<Object>()))
                .Returns("anyvalue");
            controller.Url = urlHelperMock.Object;

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedNegotiatedContentResult<FinancialInstitutionViewModel>));
        }

        [TestMethod]
        public void FICreateWithInvalidModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);
            controller.ModelState.AddModelError("test", "TEST");

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

            // Assert
            Assert.IsNotInstanceOfType(result, typeof(OkNegotiatedContentResult<FinancialInstitutionViewModel>));
        }

        [TestMethod]
        public void FICreateWithDuplicateModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            var inner2 = new SqlExceptionBuilder().WithErrorNumber(2601).Build();
            var inner1 = new Exception("", inner2);
            contextMock.Setup(x => x.SaveChanges()).Throws(new DbUpdateException("", inner1));
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void FIGetExistsAndAuthorized()
        {
            // Arrange
            var user = CreateUser();
            var contextMock = GetContextMock(user);
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.Get(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<FinancialInstitutionViewModel>));
        }

        [TestMethod]
        public void FIGetExistsAndUnauthorized()
        {
            // Arrange
            var contextMock = GetContextMock();
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);
            controller.User = CreateUser();

            // Act
            IHttpActionResult result = controller.Get(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public void FIGetDoesNotExist()
        {
            // Arrange
            var user = CreateUser();
            var contextMock = GetContextMock(user);
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.Get(2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void FiGetAllNonEmptySet()
        {
            // Arrange
            var user = CreateUser();
            var contextFake = GetContextMock(user);
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextFake.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.Get();
            var castResult = (OkNegotiatedContentResult<List<FinancialInstitutionViewModel>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<FinancialInstitutionViewModel>>));
            Assert.IsTrue(castResult.Content.Count > 0);
        }

        [TestMethod]
        public void FiGetAllEmptySet()
        {
            // Arrange
            var user = CreateUser();
            var contextFake = GetContextMock(CreateUser());
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextFake.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.Get();
            var castResult = (OkNegotiatedContentResult<List<FinancialInstitutionViewModel>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<FinancialInstitutionViewModel>>));
            Assert.IsTrue(castResult.Content.Count == 0);
        }

        //update record
        //  not authorized
        //  not found
        //  dbUpdateException
        //  OK
        [TestMethod]
        public void FIUpdateOK()
        {
            // Arrange
            var user = CreateUser();
            var bindingModel = GetValidUpdateBindingModel();
            bindingModel.Name = "different name";
            var contextMock = GetContextMock(user);
            var controller = new FinancialInstitutionsController(contextMock.Object);
            controller.User = user;

            // Act
            var result = controller.Update(bindingModel.Id, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void FIUpdateNotFound()
        {
            // Arrange
            var bindingModel = GetValidUpdateBindingModel();
            var contextMock = GetContextMock();
            var controller = new FinancialInstitutionsController(contextMock.Object);

            // Act
            var result = controller.Update(-1, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void FIUpdateDbUpdateException()
        {
            // Arrange
            var user = CreateUser();
            var bindingModel = GetValidUpdateBindingModel();
            bindingModel.Name = "different name";
            var contextMock = GetContextMock(user);
            var controller = new FinancialInstitutionsController(contextMock.Object);
            controller.User = user;
            contextMock.Setup(x => x.SaveChanges()).Throws(new DbUpdateException("some message"));

            // Act
            var result = controller.Update(bindingModel.Id, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void FIUpdateNotAuthorized()
        {
            // Arrange
            var user = CreateUser();
            var bindingModel = GetValidUpdateBindingModel();
            bindingModel.Name = "different name";
            var contextMock = GetContextMock();
            var controller = new FinancialInstitutionsController(contextMock.Object);
            controller.User = user;

            // Act
            var result = controller.Update(bindingModel.Id, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        private FinancialInstitutionCreateBindingModel GetValidBindingModel()
        {
            return new FinancialInstitutionCreateBindingModel()
            {
                Name = "My new FI",
                OfxFid = 9876,
                OfxOrg = "FI Org Name",
                OfxUrl = "https://ofx.bank.com",
                Username = "myUsername",
                Password = "mySuperSecurePassword",
                ConfirmPassword = "mySuperSecurePassword"
            };
        }

        private FinancialInstitutionUpdateBindingModel GetValidUpdateBindingModel()
        {
            return new FinancialInstitutionUpdateBindingModel()
            {
                Id = 1,
                Name = "My new FI",
                OfxFid = 9876,
                OfxOrg = "FI Org Name",
                OfxUrl = "https://ofx.bank.com"
            };
        }

        private IPrincipal CreateUser()
        {
            // create user mock
            var userMock = new Moq.Mock<IPrincipal>();

            // Create a fake Identity
            // Cannot use Moq since GetUserId() is an extension method
            string userId = Guid.NewGuid().ToString();
            List<Claim> claims = new List<Claim>
                {
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId)
                };
            var identityMock = new ClaimsIdentity(claims);
            userMock.SetupGet(x => x.Identity).Returns(identityMock);

            // assign to field
            return userMock.Object;
        }

        private Moq.Mock<IApplicationDbContext> GetContextMock(IPrincipal user = null)
        {
            // create user if needed
            if (user == null)
            {
                user = CreateUser();
            }

            // create data set
            var entityWithId = ModelMapper.BindingToEntity(GetValidBindingModel(), user);
            entityWithId.Id = 1;
            var data = new List<FinancialInstitutionModel>
            {
                entityWithId
            }.AsQueryable();

            // mock data set
            var fiMock = new Moq.Mock<DbSet<FinancialInstitutionModel>>();
            fiMock.Setup(x => x.Find(Moq.It.Is<int>(v => v.Equals(1)))).Returns(entityWithId);
            fiMock.Setup(x => x.Add(Moq.It.IsAny<FinancialInstitutionModel>())).Returns(entityWithId);
            fiMock.As<IQueryable<FinancialInstitutionModel>>().Setup(m => m.Provider).Returns(data.Provider);
            fiMock.As<IQueryable<FinancialInstitutionModel>>().Setup(m => m.Expression).Returns(data.Expression);
            fiMock.As<IQueryable<FinancialInstitutionModel>>().Setup(m => m.ElementType).Returns(data.ElementType);
            fiMock.As<IQueryable<FinancialInstitutionModel>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            // mock context
            var contextMock = new Moq.Mock<IApplicationDbContext>();
            contextMock.SetupGet(x => x.FinancialInstitutions).Returns(fiMock.Object);
            contextMock.Setup(x => x.SaveChanges()).Returns(1);

            return contextMock;
        }
    }
}

