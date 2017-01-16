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
using Budget.API.Tests.Fakes;
using Moq;
using Budget.API.Tests.FakesAndMocks;
using Microsoft.AspNet.Identity;

namespace Budget.API.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTests
    {
        #region Create Tests
        [TestMethod]
        public void AccountCreateWithValidModel()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            AccountController controller = new AccountController(contextMock.Object);
            var urlHelperMock = new Mock<UrlHelper>();
            urlHelperMock.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<Object>()))
                .Returns("anyvalue");
            controller.Url = urlHelperMock.Object;
            controller.User = user;
            AccountBindingModel model = (GetValidBindingModel());

            // Act
            IHttpActionResult result = controller.Create(model.FinancialInstitutionId, model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedNegotiatedContentResult<AccountViewModel>));
        }

        [TestMethod]
        public void AccountCreateWithInvalidModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            AccountController controller = new AccountController(contextMock.Object);
            controller.ModelState.AddModelError("test", "TEST");
            AccountBindingModel model = (GetValidBindingModel());

            // Act
            IHttpActionResult result = controller.Create(model.FinancialInstitutionId, model);

            // Assert
            Assert.IsNotInstanceOfType(result, typeof(OkNegotiatedContentResult<AccountViewModel>));
        }

        [TestMethod]
        public void AccountCreateWithDuplicateModel()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            var inner2 = new SqlExceptionBuilder().WithErrorNumber(2601).Build();
            var inner1 = new Exception("", inner2);
            contextMock.Setup(x => x.SaveChanges()).Throws(new DbUpdateException("", inner1));
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = user;
            AccountBindingModel model = (GetValidBindingModel());

            // Act
            IHttpActionResult result = controller.Create(model.FinancialInstitutionId, model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }
        #endregion

        #region Get Tests
        [TestMethod]
        public void AccountGetExistsAndAuthorized()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.Get(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<AccountViewModel>));
        }

        [TestMethod]
        public void AccountGetExistsAndUnauthorized()
        {
            // Arrange
            var contextMock = GetContextMock();
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = UserBuilder.CreateUser();

            // Act
            IHttpActionResult result = controller.Get(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public void AccountGetDoesNotExist()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.Get(-1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        #endregion

        #region GetAll Tests
        [TestMethod]
        public void AccountGetAllNonEmptySet()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextFake = GetContextMock(user);
            AccountController controller = new AccountController(contextFake.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.GetAll();
            var castResult = (OkNegotiatedContentResult<List<object>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<object>>));
            Assert.IsTrue(castResult.Content.Count > 0);
            Assert.IsInstanceOfType(castResult.Content[0], typeof(AccountViewModel));
        }

        [TestMethod]
        public void AccountGetAllEmptySet()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextFake = GetContextMock(UserBuilder.CreateUser());
            AccountController controller = new AccountController(contextFake.Object);
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
        public void AccountUpdateOK()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var bindingModel = GetValidUpdateBindingModel();
            bindingModel.Name = "different name";
            var contextMock = GetContextMock(user);
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = user;

            // Act
            var result = controller.Update(1, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void AccountUpdateNotFound()
        {
            // Arrange
            var bindingModel = GetValidUpdateBindingModel();
            var contextMock = GetContextMock();
            AccountController controller = new AccountController(contextMock.Object);

            // Act
            var result = controller.Update(-1, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void AccountUpdateDbUpdateException()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var bindingModel = GetValidUpdateBindingModel();
            bindingModel.Name = "different name";
            var contextMock = GetContextMock(user);
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = user;
            contextMock.Setup(x => x.SaveChanges()).Throws(new DbUpdateException("some message"));

            // Act
            var result = controller.Update(1, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void AccountUpdateNotAuthorized()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var bindingModel = GetValidUpdateBindingModel();
            bindingModel.Name = "different name";
            var contextMock = GetContextMock();
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = user;

            // Act
            var result = controller.Update(1, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }
        #endregion

        private AccountBindingModel GetValidBindingModel()
        {
            return new AccountBindingModel()
            {
                FinancialInstitutionId = 1,
                Number = "A123",
                Name = "The account name",
                Type = AccountType.Savings,
                Description = "Some account description"
            };
        }

        private AccountBindingModel GetValidUpdateBindingModel()
        {
            return new AccountBindingModel()
            {
                FinancialInstitutionId = 1,
                Number = "A1234",
                Name = "The new account name",
                Type = AccountType.Checking,
                Description = "Some new account description"
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
            var entityWithId1 = ModelMapper.BindingToEntity(GetValidBindingModel(), user);
            entityWithId1.Id = 1;
            var entityWithId2 = ModelMapper.BindingToEntity(GetValidBindingModel(), user);
            entityWithId2.Id = 2;
            var entityWithId3 = ModelMapper.BindingToEntity(GetValidBindingModel(), user);
            entityWithId2.Id = 3;
            var data = new List<AccountModel>
            {
                entityWithId1, entityWithId2, entityWithId3
            };

            var fiData = new List<FinancialInstitutionModel>()
            {
                new FinancialInstitutionModel()
                {
                    Id = 1,
                    UserId = user.Identity.GetUserId()
                },
                new FinancialInstitutionModel()
                {
                    Id = 2,
                    UserId = "id2"
                },
                new FinancialInstitutionModel()
                {
                    Id = 3,
                    UserId = "id3"
                }
            };

            // mock context
            var contextMockBuilder = new MockDbContext()
                .WithData(fiData)
                .WithData(data)
                .SetupAdd(entityWithId1, entityWithId1)
                .SetupFind(1, entityWithId1)
                .SetupFind(2, entityWithId2)
                .SetupFind(3, entityWithId3)
                .SetupSaveChanges(1)
                .Finalize();

            return contextMockBuilder.Context;
        }
    }
}

