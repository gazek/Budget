using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Budget.API.Controllers;
using Budget.DAL;
using Budget.API.Models;
using Budget.API.Services;
using System.Security.Principal;
using System.Collections.Generic;
using Budget.DAL.Models;
using System.Web.Http.Results;
using System.Web.Http;
using System.Data.Entity.Infrastructure;
using System.Web.Http.Routing;
using System.Linq;
using Budget.API.Tests.Fakes;
using Moq;
using Budget.API.Tests.FakesAndMocks;
using System.Net.Http;

namespace Budget.API.Tests.Controllers
{
    [TestClass]
    public class FinancialInstitutionsControllerTests
    {
        #region Create Tests
        [TestMethod]
        public void FICreateWithValidModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
            var urlHelperMock = new Mock<UrlHelper>();
            urlHelperMock.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<Object>()))
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
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
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
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }
        #endregion

        #region Get Tests
        [TestMethod]
        public void FIGetExistsAndAuthorized()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
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
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = UserBuilder.CreateUser();

            // Act
            IHttpActionResult result = controller.Get(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public void FIGetDoesNotExist()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.Get(2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region GetAll Tests
        [TestMethod]
        public void FiGetAllNonEmptySet()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextFake = GetContextMock(user);
            FinancialInstitutionController controller = new FinancialInstitutionController(contextFake.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.GetAll();
            var castResult = (OkNegotiatedContentResult<List<object>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<object>>));
            Assert.IsTrue(castResult.Content.Count > 0);
            Assert.IsInstanceOfType(castResult.Content[0], typeof(FinancialInstitutionViewModel));

        }

        [TestMethod]
        public void FiGetAllEmptySet()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextFake = GetContextMock(UserBuilder.CreateUser());
            FinancialInstitutionController controller = new FinancialInstitutionController(contextFake.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.GetAll();
            var castResult = (OkNegotiatedContentResult<List<object>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<object>>));
            Assert.IsTrue(castResult.Content.Count == 0);
        }
        #endregion

        #region Update Tests
        [TestMethod]
        public void FIUpdateOK()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var bindingModel = GetValidUpdateBindingModel();
            bindingModel.Name = "different name";
            var contextMock = GetContextMock(user);
            var controller = new FinancialInstitutionController(contextMock.Object);
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
            var controller = new FinancialInstitutionController(contextMock.Object);

            // Act
            var result = controller.Update(-1, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void FIUpdateDbUpdateException()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var bindingModel = GetValidUpdateBindingModel();
            bindingModel.Name = "different name";
            var contextMock = GetContextMock(user);
            var controller = new FinancialInstitutionController(contextMock.Object);
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
            var user = UserBuilder.CreateUser();
            var bindingModel = GetValidUpdateBindingModel();
            bindingModel.Name = "different name";
            var contextMock = GetContextMock();
            var controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = user;

            // Act
            var result = controller.Update(bindingModel.Id, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }
        #endregion

        #region Update Login Credentials Tests
        [TestMethod]
        public void FIUpdateLoginOK()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var createBindingModel = GetValidBindingModel();
            var loginUpdateBindingModel = GetValidUpdateLoginBindingModel();
            loginUpdateBindingModel.Username = "different name";
            var contextMock = GetContextMock(user);
            var controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = user;

            // Act
            var result = controller.UpdateLogin(1, loginUpdateBindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreNotEqual(contextMock.Object.FinancialInstitutions.Find(1).Username, createBindingModel.Username);
            Assert.AreNotEqual(AesService.DecryptStringFromBytes(contextMock.Object.FinancialInstitutions.Find(1).PasswordHash), createBindingModel.Password);
        }

        [TestMethod]
        public void FIUpdateLoginNotFound()
        {
            // Arrange
            var bindingModel = GetValidUpdateLoginBindingModel();
            var contextMock = GetContextMock();
            var controller = new FinancialInstitutionController(contextMock.Object);

            // Act
            var result = controller.UpdateLogin(-1, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void FIUpdateLoginDbUpdateException()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var bindingModel = GetValidUpdateLoginBindingModel();
            var contextMock = GetContextMock(user);
            var controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = user;
            contextMock.Setup(x => x.SaveChanges()).Throws(new DbUpdateException("some message"));

            // Act
            var result = controller.UpdateLogin(1, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void FIUpdateLoginNotAuthorized()
        {
            // Arrange
            var bindingModel = GetValidUpdateLoginBindingModel();
            var contextMock = GetContextMock();
            var controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = UserBuilder.CreateUser();

            // Act
            var result = controller.UpdateLogin(1, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }
        #endregion

        #region Get Account List Tests
        [TestMethod]
        public void FIGetAccountListDoesNotExist()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.GetAccountList(2);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        
        [TestMethod]
        public void FIGetAccountListExistsAndUnauthorized()
        {
            // Arrange
            var contextMock = GetContextMock();
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = UserBuilder.CreateUser();

            // Act
            IHttpActionResult result = controller.GetAccountList(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }
        
        [TestMethod]
        public void FIGetAccountListOfxRequestFails()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = user;
            var ofx = new MockOfxClient().WithFailedRequest();
            controller.OfxClient = ofx.GetMock();

            // Act
            IHttpActionResult result = controller.GetAccountList(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }
        
        [TestMethod]
        public void FIGetAccountListOfxRequestSucceedsNoOfxString()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = user;
            var ofx = new MockOfxClient().WithSuccessfulRequest(null);
            controller.OfxClient = ofx.GetMock();

            // Act
            IHttpActionResult result = controller.GetAccountList(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(InternalServerErrorResult));
        }

        [TestMethod]
        public void FIGetAccountListOfxSignonRequestFails()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = user;
            var ofx = new MockOfxClient().WithSuccessfulRequest("fake ofx string").WithSignonFailure("fake error message");
            controller.OfxClient = ofx.GetMock();

            // Act
            IHttpActionResult result = controller.GetAccountList(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void FIGetAccountListSuccess()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            FinancialInstitutionController controller = new FinancialInstitutionController(contextMock.Object);
            controller.User = user;
            var ofx = new MockOfxClient()
                .WithSuccessfulRequest("fake ofx string")
                .WithSignonSuccess()
                .WithAccounts(new List<AccountModel>());
            controller.OfxClient = ofx.GetMock();

            // Act
            IHttpActionResult result = controller.GetAccountList(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<IEnumerable<AccountListViewModel>>));
        }


        #endregion

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

        private FinancialInstitutionUpdateLoginBindingModel GetValidUpdateLoginBindingModel()
        {
            return new FinancialInstitutionUpdateLoginBindingModel()
            {
                Username = "userNumberOne",
                Password = "AGreatAndSecurePassword",
                ConfirmPassword = "AGreatAndSecurePassword"
            };
        }

        private Mock<IApplicationDbContext> GetContextMock(IPrincipal user = null)
        {
            // create user if needed
            if (user == null)
            {
                user = UserBuilder.CreateUser();
            }

            // create data set
            var entityWithId = ModelMapper.BindingToEntity(GetValidBindingModel(), user);
            entityWithId.Id = 1;
            var data = new List<FinancialInstitutionModel>
            {
                entityWithId
            };

            // mock context
            var contextMockBuilder = new MockDbContext()
                .WithData(data)
                .SetupAdd(entityWithId, entityWithId)
                .SetupFind(1, entityWithId)
                .SetupSaveChanges(1)
                .Finalize();

            return contextMockBuilder.Context;
        }
    }
}

