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
using System.Text;
using System.Threading.Tasks;

namespace Budget.API.Tests.Services
{
    [TestClass]
    public class TransactionsDetailsCheckerTests
    {
        [TestMethod]
        public void TransDetailsCheckerAmountIsFullyCategorized()
        {
            // Arrange
            var trans = GetTransactionDetailsOk("fake");
            var dbContext = new MockDbContext()
                .GetMock();

            // Act
            var checker = new TransactionDetailsChecker(trans, dbContext);

            //Assert
            Assert.IsTrue(checker.AmountIsFullyCategorized);
        }

        [TestMethod]
        public void TransDetailsCheckerAmountIsNotFullyCategorized()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var trans = GetTransactionDetailsNotOk(user.Identity.GetUserId());
            var dbContext = MakeContext(user).Object;

            // Act
            var checker = new TransactionDetailsChecker(trans, dbContext);

            //Assert
            Assert.IsFalse(checker.AmountIsFullyCategorized);
        }


        [TestMethod]
        public void TransDetailsCheckerDetailKeysAreUnique()
        {
            // Arrange
            var trans = GetTransactionDetailsOk("fake");
            var dbContext = new MockDbContext()
                .GetMock();

            // Act
            var checker = new TransactionDetailsChecker(trans, dbContext);

            //Assert
            Assert.IsTrue(checker.DetailsKeysAreUnique);
        }

        [TestMethod]
        public void TransDetailsCheckerDetailKeysAreNotUnique()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var trans = GetTransactionDetailsNotOk(user.Identity.GetUserId());
            var dbContext = MakeContext(user).Object;

            // Act
            var checker = new TransactionDetailsChecker(trans, dbContext);

            //Assert
            Assert.IsFalse(checker.DetailsKeysAreUnique);
        }

        private AccountModel CreateAccount(string userId)
        {
            return new AccountModel()
            {
                Id = 11,
                FinancialInstitutionId = 99,
                FinancialInstitution = new FinancialInstitutionModel()
                {
                    Id = 99,
                    UserId = userId
                }
            };
        }

        private TransactionModel GetTransactionDetailsOk(string userId)
        {
            var detail1 = new TransactionDetailModel()
            {
                Amount = 2.3m,
                CategoryId = 1,
                SubCategoryId = 1
            };

            var detail2 = new TransactionDetailModel()
            {
                Amount = 10.04m,
                CategoryId = 1,
                SubCategoryId = 2
            };

            var details = new List<TransactionDetailModel>() { detail1, detail2 };

            var trans = new TransactionModel()
            {
                AccountId = CreateAccount(userId).Id,
                Amount = 12.34m,
                Details = details,
                Account = CreateAccount(userId)
            };

            return trans;
        }

        private TransactionModel GetTransactionDetailsNotOk(string userId)
        {
            var trans = GetTransactionDetailsOk(userId);
            trans.Details.LastOrDefault().SubCategoryId = 1;
            trans.Amount = 23.45m;
            return trans;
        }

        private Mock<IApplicationDbContext> MakeContext(IPrincipal user)
        {
            var unPayee = new PayeeModel()
            {
                Id = 99,
                UserId = user.Identity.GetUserId(),
                Name = "Unassigned",
                ImportNames = new List<PayeeImportNameModel>(),
                DefaultDetails = new List<PayeeDefaultDetailModel>()
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

            var account = CreateAccount(user.Identity.GetUserId());
            var financialInstitution = account.FinancialInstitution;
            // mock context
            var contextMockBuilder = new MockDbContext()
                .WithData(new List<AccountModel>() { account })
                .WithData(new List<FinancialInstitutionModel>() { financialInstitution })
                .WithData(new List<PayeeModel>() { unPayee })
                .WithData(new List<CategoryModel>() { unCat })
                .SetupFind(account.Id, account)
                .SetupFind(financialInstitution.Id, financialInstitution)
                .Finalize();

            return contextMockBuilder.Context;
        }
    }
}
