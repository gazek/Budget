﻿using System;
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

namespace Budget.API.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        #region Create Tests
        [TestMethod]
        public void AccountCreateWithValidModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            AccountController controller = new AccountController(contextMock.Object);
            var urlHelperMock = new Mock<UrlHelper>();
            urlHelperMock.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<Object>()))
                .Returns("anyvalue");
            controller.Url = urlHelperMock.Object;

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

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

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

            // Assert
            Assert.IsNotInstanceOfType(result, typeof(OkNegotiatedContentResult<AccountViewModel>));
        }

        [TestMethod]
        public void AccountCreateWithDuplicateModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            var inner2 = new SqlExceptionBuilder().WithErrorNumber(2601).Build();
            var inner1 = new Exception("", inner2);
            contextMock.Setup(x => x.SaveChanges()).Throws(new DbUpdateException("", inner1));
            AccountController controller = new AccountController(contextMock.Object);

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

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
            IHttpActionResult result = controller.Get();
            var castResult = (OkNegotiatedContentResult<List<AccountViewModel>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<AccountViewModel>>));
            Assert.IsTrue(castResult.Content.Count > 0);
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
            IHttpActionResult result = controller.Get();
            var castResult = (OkNegotiatedContentResult<List<AccountViewModel>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<AccountViewModel>>));
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
            var result = controller.Update(bindingModel.Id, bindingModel);

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
            var result = controller.Update(bindingModel.Id, bindingModel);

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
            var result = controller.Update(bindingModel.Id, bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }
        #endregion

        #region Get Balance History
        [TestMethod]
        public void AccountGetBalanceHistoryUnauthorized()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = UserBuilder.CreateUser();

            // Act
            var result = controller.GetBalanceHistory(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public void AccountGetBalanceHistoryAccountNotFoumd()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = user;

            // Act
            var result = controller.GetBalanceHistory(-1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void AccountGetBalanceHistoryEmptyResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.GetBalanceHistory(3);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<BalanceViewModel>>));
            var typedResult = (OkNegotiatedContentResult<List<BalanceViewModel>>)result;
            Assert.AreEqual(0, typedResult.Content.Count);
        }

        [TestMethod]
        public void AccountGetBalanceHistoryNonEmptyResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            AccountController controller = new AccountController(contextMock.Object);
            controller.User = user;

            // Act
            IHttpActionResult result = controller.GetBalanceHistory(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<BalanceViewModel>>));
            var typedResult = (OkNegotiatedContentResult<List<BalanceViewModel>>)result;
            Assert.AreEqual(2, typedResult.Content.Count);
        }

        [TestMethod]
        public void NEED_AccountGetBalanceHistoryDateRange()
        {
            // need to write this test...and the underlying method
            Assert.IsTrue(false);
        }
        #endregion

        #region Pull Latest Transactions
        [TestMethod]
        public void NEED_AccountPullTransactionsFromBank()
        {
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_AccountPullTransactionsFromBankBalance()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_AccountLatestTransactionsFromDB()
        {
            Assert.IsTrue(false);
        }
        #endregion

        private AccountBindingModel GetValidBindingModel()
        {
            return new AccountBindingModel()
            {
                Id = 0,
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
                Id = 1,
                FinancialInstitutionId = 1,
                Number = "A1234",
                Name = "The new account name",
                Type = AccountType.Checking,
                Description = "Some new account description"
            };
        }

        private List<BalanceModel> GetBalanceList()
        {
            List<BalanceModel> dataset = new List<BalanceModel>();
            dataset.Add(new BalanceModel()
            {
                Id = 2,
                AccountId = 1,
                AsOfDate = DateTime.Parse("01/01/2016"),
                Amount = 1234.56M
            });

            dataset.Add(new BalanceModel()
            {
                Id = 3,
                AccountId = 1,
                AsOfDate = DateTime.Parse("01/02/2016"),
                Amount = 7890.12M
            });

            dataset.Add(new BalanceModel()
            {
                Id = 5,
                AccountId = 2,
                AsOfDate = DateTime.Parse("01/02/2016"),
                Amount = 7890.12M
            });

            return dataset;
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

            // mock context
            var contextMockBuilder = new MockDbContext()
                .WithData(data)
                .WithData(GetBalanceList())
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

