﻿using Budget.API.Controllers;
using Budget.API.Models;
using Budget.API.Services;
using Budget.API.Tests.Fakes;
using Budget.API.Tests.FakesAndMocks;
using Budget.DAL;
using Budget.DAL.Models;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Results;

namespace Budget.API.Tests.Controllers
{
    [TestClass]
    public class BalanceControllerTests
    {
        [TestMethod]
        public void BalanceGetHistoryUnauthorized()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            BalanceController controller = new BalanceController(contextMock.Object);
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
            BalanceController controller = new BalanceController(contextMock.Object);
            controller.User = user;
            controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://some.url/?startDate=2017-08-01&endDate=2017-08-04");

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
            BalanceController controller = new BalanceController(contextMock.Object);
            controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://some.url/?startDate=2017-08-01&endDate=2017-08-04");
            controller.User = user;

            // Act
            IHttpActionResult result = controller.GetBalanceHistory(3);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<object>>));
            var typedResult = (OkNegotiatedContentResult<List<object>>)result;
            Assert.AreEqual(0, typedResult.Content.Count);
        }

        [TestMethod]
        public void AccountGetBalanceHistoryNonEmptyResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            BalanceController controller = new BalanceController(contextMock.Object);
            controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://some.url/?startDate=2017-08-01&endDate=2017-08-04");
            controller.User = user;

            // Act
            IHttpActionResult result = controller.GetBalanceHistory(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<object>>));
            var typedResult = (OkNegotiatedContentResult<List<object>>)result;
            Assert.AreEqual(2, typedResult.Content.Count);
            Assert.IsInstanceOfType(typedResult.Content.First(), typeof(BalanceViewModel));
            DateTime asOf1 = (DateTime)typedResult.Content[0].GetType().GetProperty("AsOfDate").GetValue(typedResult.Content[0]);
            DateTime asOf2 = (DateTime)typedResult.Content[1].GetType().GetProperty("AsOfDate").GetValue(typedResult.Content[1]);
            Assert.IsTrue(asOf1 < asOf2);
        }

        [TestMethod]
        public void AccountGetBalanceHistoryDateRange()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = GetContextMock(user);
            BalanceController controller = new BalanceController(contextMock.Object);
            controller.Request = new HttpRequestMessage(HttpMethod.Get, "http://some.url/?startDate=2017-08-01&endDate=2017-08-04");
            controller.User = user;

            // Act
            IHttpActionResult result = controller.GetBalanceHistory(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<object>>));
            var typedResult = (OkNegotiatedContentResult<List<object>>)result;
            Assert.IsInstanceOfType(typedResult.Content.First(), typeof(BalanceViewModel));
            Assert.AreEqual(1, typedResult.Content.Count);
        }

        private Mock<IApplicationDbContext> GetContextMock(IPrincipal user = null)
        {
            // create user if needed
            if (user == null)
            {
                user = UserBuilder.CreateUser();
            }

            // create financial institutions
            var fi1 = new FinancialInstitutionModel() { Id = 1, UserId = user.Identity.GetUserId() };
            var fi2 = new FinancialInstitutionModel() { Id = 2, UserId = "id2" };
            var fi3 = new FinancialInstitutionModel() { Id = 3, UserId = "id3" };
            var fiData = new List<FinancialInstitutionModel>() { fi1, fi2, fi3 };

            // create data set
            var entityWithId1 = ModelMapper.BindingToEntity(GetValidBindingModel(), fi1);
            entityWithId1.Id = 1;
            var entityWithId2 = ModelMapper.BindingToEntity(GetValidBindingModel(), fi1);
            entityWithId2.Id = 2;
            var entityWithId3 = ModelMapper.BindingToEntity(GetValidBindingModel(), fi1);
            entityWithId2.Id = 3;
            var data = new List<AccountModel>
            {
                entityWithId1, entityWithId2, entityWithId3
            };

            // mock context
            var contextMockBuilder = new MockDbContext()
                .WithData(fiData)
                .SetupFind(1, fi1)
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
    }
}
