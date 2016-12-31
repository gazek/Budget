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
                Status = TransactionStatus.Rejected,
                Details = new List<TransactionDetailBindingModel>() { detail }
            };
            controller.User = user;

            // Act
            var result = controller.Update(1, updatedTrans);
            OkNegotiatedContentResult<TransactionViewModel> resultTyped = (OkNegotiatedContentResult<TransactionViewModel>)result;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<TransactionViewModel>));
            Assert.AreEqual(detail.CategoryId, resultTyped.Content.Details.First().CategoryId);
            Assert.AreEqual(detail.Memo, resultTyped.Content.Details.First().Memo);
            Assert.AreEqual(detail.PayeeId, resultTyped.Content.Details.First().PayeeId);
            Assert.AreEqual(detail.SubCategoryId, resultTyped.Content.Details.First().SubCategoryId);
        }

        [TestMethod]
        public void TransactionControllerUpdateByIdBadRequestNonUniqueDetailsResult()
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
                Status = TransactionStatus.Rejected,
                Details = new List<TransactionDetailBindingModel>() { detail, detail }
            };
            controller.User = user;

            // Act
            var result = controller.Update(1, updatedTrans);
            BadRequestErrorMessageResult resultTyped = (BadRequestErrorMessageResult)result;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            Assert.AreEqual("Payee, Category, Subcategory combination must be unique", resultTyped.Message);
        }

        [TestMethod]
        public void TransactionControllerUpdateByIdBadRequestOkResultWithUncatAdded()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var dbContext = MakeContext(user).Object;
            var controller = new TransactionController(dbContext);
            var originalTrans = dbContext.Transactions.Where(t => t.Id == 1).FirstOrDefault();
            var unCatAmount = 1.23m;
            var detail1 = new TransactionDetailBindingModel()
            {
                Amount = originalTrans.Amount - unCatAmount,
                CategoryId = 99,
                Memo = "foo",
                PayeeId = 100,
                SubCategoryId = 101
            };
            var updatedTrans = new TransactionBindingModel()
            {
                CheckNum = 1,
                Status = TransactionStatus.Rejected,
                Details = new List<TransactionDetailBindingModel>() { detail1 }
            };
            controller.User = user;

            // Act
            var result = controller.Update(1, updatedTrans);
            OkNegotiatedContentResult<TransactionViewModel> resultTyped = (OkNegotiatedContentResult<TransactionViewModel>)result;
            var unCatDetail = resultTyped.Content.Details.Where(d => d.Amount == unCatAmount).FirstOrDefault();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<TransactionViewModel>));
            Assert.IsNotNull(unCatDetail);
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
            var resultTyped = (OkNegotiatedContentResult<List<TransactionModel>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<TransactionModel>>));
            Assert.AreEqual(1, resultTyped.Content.Count);
            Assert.IsTrue(resultTyped.Content.FirstOrDefault().Date >= startDate);
            Assert.IsTrue(resultTyped.Content.FirstOrDefault().Date <= endDate);
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
            var resultTyped = (OkNegotiatedContentResult<List<TransactionModel>>)result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<List<TransactionModel>>));
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
            var account1 = new AccountModel()
            {
                UserId = user.Identity.GetUserId()
            };

            var account2 = new AccountModel()
            {
                UserId = UserBuilder.CreateUser().Identity.GetUserId()
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
                    UserId = UserBuilder.CreateUser().Identity.GetUserId()
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
                .Finalize();

            return contextMockBuilder.Context;
        }
    }
}
