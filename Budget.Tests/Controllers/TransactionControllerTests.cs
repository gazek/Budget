using Budget.API.Controllers;
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
using System.Security.Principal;
using System.Web.Http.Results;

namespace Budget.API.Tests.Controllers
{
    [TestClass]
    public class TransactionControllerTests
    {
        #region By ID
        #region Get
        [TestMethod]
        public void TransactionControllerGetByIdFound()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            controller.User = user;

            // Act
            var result = controller.Get(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<TransactionViewModel>));
        }

        [TestMethod]
        public void TransactionControllerGetByIdNotFound()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            controller.User = user;

            // Act
            var result = controller.Get(7);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void TransactionControllerGetByIdUnAuthorized()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            controller.User = user;

            // Act
            var result = controller.Get(3);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }
        #endregion

        #region Put
        [TestMethod]
        public void TransactionControllerUpdateByIdOkResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            var originalTrans = dbContext.Transactions.Where(t => t.Id == 1).FirstOrDefault();
            var detail = new TransactionDetailBindingModel()
            {
                Amount = originalTrans.Amount,
                CategoryId = 99,
                Memo = "foo",
                PayeeId = 100,
                SubCategoryId = 101
            };
            var updatedTrans = new TransactionBindingModel()
            {
                CheckNum = 1,
                Status = TransactionStatus.Rejected
            };
            controller.User = user;

            // Act
            var result = controller.Update(1, updatedTrans);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void TransactionControllerUpdateByIdNotFoundResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            controller.User = user;

            // Act
            var result = controller.Update(99, new TransactionBindingModel());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void TransactionControllerUpdateByIdUnAuthorizedResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            controller.User = user;

            // Act
            var result = controller.Update(3, new TransactionBindingModel());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }
        #endregion
        #endregion

        #region By Account

        #region Date Range
        [TestMethod]
        public void TransactionControllerGetByAccountIdAndDateRangeWithResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            var startDate = DateTime.Today.AddDays(-20);
            var endDate = DateTime.Today.AddDays(-8);
            controller.User = user;

            // Act
            var result = controller.GetTransactionsFromDb(1, startDate.ToShortDateString(), endDate.ToShortDateString());
            var resultTyped = (OkNegotiatedContentResult<List<object>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<object>>));
            Assert.AreEqual(1, resultTyped.Content.Count);
            Assert.IsInstanceOfType(resultTyped.Content.First(), typeof(TransactionViewModel));
            TransactionViewModel first = resultTyped.Content.FirstOrDefault() as TransactionViewModel;
            Assert.IsTrue(first.Date >= startDate);
            Assert.IsTrue(first.Date <= endDate);
        }

        [TestMethod]
        public void TransactionControllerGetByAccountIdAndDateRangeWithEmptyResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            var startDate = DateTime.Today.AddDays(-200);
            var endDate = DateTime.Today.AddDays(-80);
            controller.User = user;

            // Act
            var result = controller.GetTransactionsFromDb(1, startDate.ToShortDateString(), endDate.ToShortDateString());
            var resultTyped = (OkNegotiatedContentResult<List<object>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<object>>));
            Assert.AreEqual(0, resultTyped.Content.Count);
        }

        [TestMethod]
        public void TransactionControllerGetByAccountIdAndDateRangeNotFoundResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            var startDate = DateTime.Today.AddDays(-200);
            var endDate = DateTime.Today.AddDays(-80);
            controller.User = user;

            // Act
            var result = controller.GetTransactionsFromDb(5, startDate.ToShortDateString(), endDate.ToShortDateString());

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void TransactionControllerGetByAccountIdAndDateRangeUnAuthorizedResult()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            var startDate = DateTime.Today.AddDays(-200);
            var endDate = DateTime.Today.AddDays(0);
            controller.User = user;

            // Act
            var result = controller.GetTransactionsFromDb(2, startDate.ToShortDateString(), endDate.ToShortDateString());

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }
        #endregion

        [TestMethod]
        public void NEED_TransactionPullTransactionsFromBank()
        {
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_TransactionPullTransactionsFromBankVerifyBalance()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        #endregion

        private Mock<IApplicationDbContext> MakeContext(IPrincipal user)
        {
            var fi1 = new FinancialInstitutionModel()
            {
                Id = 1,
                UserId = user.Identity.GetUserId()
            };
            var fi2 = new FinancialInstitutionModel()
            {
                Id = 2,
                UserId = UserBuilder.CreateUser().Identity.GetUserId()
            };
            var fi3 = new FinancialInstitutionModel()
            {
                Id = 3,
                UserId = UserBuilder.CreateUser().Identity.GetUserId()
            };
            var account1 = new AccountModel()
            {
                FinancialInstitutionId = fi1.Id,
                Id = 1
            };

            var account2 = new AccountModel()
            {
                FinancialInstitutionId = fi2.Id,
                Id = 2
            };

            var trans1 = new TransactionModel()
            {
                Id = 1,
                AccountId = 1,
                Account = account1,
                Amount = 12.34m,
                Date = DateTime.Now.AddDays(-10),
                LastEditDate = DateTime.Now.AddDays(-10),
                Status = TransactionStatus.New,
                Details = new List<TransactionDetailModel>()
                {
                    new TransactionDetailModel()
                    {
                        Amount = 12.34m,
                        CategoryId = 1,
                        LastEditDate = DateTime.Now.AddDays(-10),
                        Memo = "",
                        PayeeId = 1,
                        SubCategoryId = 1,
                        TransactionId = 1
                    }
                }
            };

            var trans2 = new TransactionModel()
            {
                Id = 2,
                AccountId = 1,
                Account = account1,
                Amount = 43.21m,
                Date = DateTime.Now.AddDays(-7),
                LastEditDate = DateTime.Now.AddDays(-7),
                Status = TransactionStatus.Attention,
                Details = new List<TransactionDetailModel>()
                {
                    new TransactionDetailModel()
                    {
                        Amount = 43.21m,
                        CategoryId = 2,
                        LastEditDate = DateTime.Now.AddDays(-7),
                        Memo = "hmmmm",
                        PayeeId = 2,
                        SubCategoryId = 2,
                        TransactionId = 2
                    }
                }
            };

            var trans3 = new TransactionModel()
            {
                Id = 3,
                AccountId = 2,
                Account = new AccountModel()
                {
                    FinancialInstitutionId = fi2.Id,
                    FinancialInstitution = fi2
                },
                Amount = 444.44m,
                Date = DateTime.Now.AddDays(-4),
                LastEditDate = DateTime.Now.AddDays(-4),
                Status = TransactionStatus.Attention,
                Details = new List<TransactionDetailModel>()
                {
                    new TransactionDetailModel()
                    {
                        Amount = 43.21m,
                        CategoryId = 2,
                        LastEditDate = DateTime.Now.AddDays(-7),
                        Memo = "hmmmm",
                        PayeeId = 2,
                        SubCategoryId = 2,
                        TransactionId = 2
                    }
                }
            };

            var unPayee = new PayeeModel()
            {
                Id = 99,
                UserId = user.Identity.GetUserId(),
                Name = "Unassigned",
                ImportNames = new List<ImportNameToPayeeModel>(),
                DefaultDetails = new List<PayeeDefaultDetailsModel>()
            };
            var unCat = new CategoryModel()
            {
                Name = "Uncategorized",
                UserId = user.Identity.GetUserId(),
                SubCategories = new List<SubCategoryModel>()
                {
                    new SubCategoryModel()
                    {
                        Name = "Uncategorized"
                    }
                }
            };

            // mock context
            var contextMockBuilder = new MockDbContext()
                .WithData(new List<PayeeModel>() { unPayee })
                .WithData(new List<CategoryModel>() { unCat })
                .WithData(new List<TransactionModel>() { trans1, trans2, trans3 })
                .WithData(new List<AccountModel>() { account1, account2 })
                .SetupFind(1, account1)
                .SetupFind(2, account2)
                .SetupFind(1, trans1)
                .SetupFind(2, trans2)
                .SetupFind(3, trans3)
                .Finalize();

            return contextMockBuilder.Context;
        }
    }
}
