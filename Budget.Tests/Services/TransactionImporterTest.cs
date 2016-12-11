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

namespace Budget.API.Tests.Services
{
    [TestClass]
    public class TransactionImporterTest
    {
        [TestMethod]
        public void TransactionImporterConstructorNoneExistingTest()
        {
            // Arrange
            var context = MakeContext().Object;
            var defaults = new TransactionDefaults(context);
            var trans = GetFakeTransactions();
            var account = GetFakeAccount(new List<TransactionModel>() { trans[0] });
            trans.RemoveAt(0);

            // Act
            var importer = new TransactionImporter(trans, account, context);

            // Assert
            Assert.AreEqual(0, importer.Transactions.Select(x => x.Id).Aggregate((acc, x) => acc + x));
        }

        [TestMethod]
        public void TransactionImporterConstructorWithExistingTest()
        {
            // Arrange
            var context = MakeContext().Object;
            var defaults = new TransactionDefaults(context);
            var trans = GetFakeTransactions();
            var account = GetFakeAccount(new List<TransactionModel>() { trans[0] });
            var sum = context.Transactions.First().Id;

            // Act
            var importer = new TransactionImporter(trans, account, context);

            // Assert
            Assert.AreEqual(sum, importer.Transactions.Select(x => x.Id).Aggregate((acc, x) => acc + x));
        }

        [TestMethod]
        public void TransactionImporterFilterExistingTest()
        {
            // Arrange
            var context = MakeContext().Object;
            var defaults = new TransactionDefaults(context);
            var trans = GetFakeTransactions();
            var account = GetFakeAccount(new List<TransactionModel>() { trans[0] });
            var sum = context.Transactions.First().Id;
            var importer = new TransactionImporter(trans, account, context);

            // Act
            importer.FilterExisting();

            // Assert
            Assert.AreEqual(3, trans.Count);
            Assert.AreEqual(2, importer.Transactions.Count);
        }

        [TestMethod]
        public void TransactionImporterApplyDefaultsDefaultValueTest()
        {
            // Arrange
            var context = MakeContext().Object;
            var defaults = new Mock<TransactionDefaults>();
            defaults.Setup(x => x.GetDefaultPayees(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<PayeeModel>());
            var trans = GetFakeTransactions();
            var account = GetFakeAccount(new List<TransactionModel>() { trans[0] });
            var importer = new TransactionImporter(trans, account, context);

            // Act
            importer.SetDefaultStatus().ApplyDefaults();


            // Assert
            foreach (TransactionModel t in importer.Transactions)
            {
                Assert.AreEqual(TransactionStatus.New, t.Status);
            }
        }

        [TestMethod]
        public void TransactionImporterApplyDefaultsCustomValueTest()
        {
            // Arrange
            var context = MakeContext().Object;
            var defaults = new Mock<TransactionDefaults>();
            defaults.Setup(x => x.GetDefaultPayees(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<PayeeModel>());
            var trans = GetFakeTransactions();
            var account = GetFakeAccount(new List<TransactionModel>() { trans[0] });
            var importer = new TransactionImporter(trans, account, context);

            // Act
            importer.SetDefaultStatus(TransactionStatus.Void).ApplyDefaults();


            // Assert
            foreach (TransactionModel t in importer.Transactions)
            {
                Assert.AreEqual(TransactionStatus.Void, t.Status);
            }
        }

        [TestMethod]
        public void TransactionImporterApplyDefaultsDoNotModifyTest()
        {
            // Arrange
            var context = MakeContext().Object;
            var defaults = new Mock<TransactionDefaults>();
            defaults.Setup(x => x.GetDefaultPayees(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<PayeeModel>());
            var trans = GetFakeTransactions();
            trans[0].Status = TransactionStatus.Rejected;
            var account = GetFakeAccount(new List<TransactionModel>() { trans[0] });
            var importer = new TransactionImporter(trans, account, context);

            // Act
            importer.UnsetDefaultStatus().ApplyDefaults();

            // Assert
            for (int i = 0; i < trans.Count; ++i)
            {
                Assert.AreEqual(trans[i].Status, importer.Transactions[i].Status);
            }
        }

        [TestMethod]
        public void NEED_TransactionImporterApplyDefaultsDetailsSingleDetailTest()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_TransactionImporterApplyDefaultsDetailsMultipleDetailTest()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_TransactionImporterCommitTest()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_TransactionImporterCommitTestAddBalance()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        private List<TransactionModel> GetFakeTransactions()
        {
            var trans1 = new TransactionModel()
            {
                ReferenceValue = "1",
                Date = DateTime.Today.AddDays(-5),
                OriginalPayeeName = "",
            };

            var trans2 = new TransactionModel()
            {
                ReferenceValue = "2",
                OriginalPayeeName = "",
                Date = DateTime.Today.AddDays(-5)
            };

            var trans3 = new TransactionModel()
            {
                ReferenceValue = "3",
                OriginalPayeeName = "",
                Date = DateTime.Today.AddDays(-4)
            };

            return new List<TransactionModel>() { trans1, trans2, trans3 };
        }

        private AccountModel GetFakeAccount(List<TransactionModel> trans)
        {
            var user = new ApplicationUser()
            {
                Id = "123"
            };
            return new AccountModel()
            {
                Id = 13,
                Transactions = trans,
                UserId = user.Id,
                User = user
            };
        }

        private Mock<IApplicationDbContext> MakeContext()
        {
            // get transactions
            var trans1 = GetFakeTransactions()[0];
            trans1.Id = 1;
            var trans = new List<TransactionModel>() { trans1 };

            // get account
            var account = GetFakeAccount(trans);

            // mock context
            var contextMockBuilder = new MockDbContext()
                .WithData(trans)
                .SetupFind(1, trans1)
                .WithData(new List<AccountModel>() { account })
                .SetupFind(13, account)
                .WithData(new List<PayeeModel>())
                .Finalize();

            return contextMockBuilder.Context;
        }
    }
}
