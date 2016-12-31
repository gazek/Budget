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
    public class TransactionImporterTests
    {
        [TestMethod]
        public void TransactionImporterConstructorNoneExistingTest()
        {
            // Arrange
            var context = MakeContext().Object;
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
            defaults.Setup(x => x.GetDefaultPayees(It.IsAny<string>())).Returns(new List<PayeeModel>());
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
            defaults.Setup(x => x.GetDefaultPayees(It.IsAny<string>())).Returns(new List<PayeeModel>());
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
            defaults.Setup(x => x.GetDefaultPayees(It.IsAny<string>())).Returns(new List<PayeeModel>());
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
        public void TransactionImporterApplyDefaultsDetailsSingleDetailTest()
        {
            // Arrange
            var defaults = MakeTransDefaults();
            var context = MakeContext().Object;
            var trans = GetFakeTransactions();
            var account = GetFakeAccount(new List<TransactionModel>() { trans[0] });
            var importer = new TransactionImporter(trans, account, context, defaults.Object);
            importer.SetDefaultDetails();

            // Act
            importer.UnsetDefaultStatus().ApplyDefaults();

            // Assert
            Assert.AreEqual(1, importer.Transactions[0].Details.Count);
        }

        [TestMethod]
        public void TransactionImporterApplyDefaultsDetailsMultipleDetailTest()
        {
            // Arrange
            var defaults = MakeTransDefaults();
            var context = MakeContext().Object;
            var trans = GetFakeTransactions();
            foreach (TransactionModel t in trans)
            {
                t.Details = null;
            }
            var account = GetFakeAccount(new List<TransactionModel>() { trans[1] });
            var importer = new TransactionImporter(trans, account, context, defaults.Object);
            importer.SetDefaultDetails();

            // Act
            importer.UnsetDefaultStatus().ApplyDefaults();

            // Assert
            Assert.AreEqual(2, importer.Transactions[1].Details.Count);
        }

        [TestMethod]
        public void TransactionImporterCommit()
        {
            // Arrange
            var context = MakeContext();
            context.Setup(x => x.SaveChanges()).Returns(3);
            var trans = GetFakeTransactions();
            var account = GetFakeAccount(new List<TransactionModel>(trans));
            var importer = new TransactionImporter(trans, account, context.Object);

            // Act
            var result = importer.Commit();

            // Assert
            Assert.AreEqual(3, result);
        }

        private List<TransactionModel> GetFakeTransactions()
        {
            var trans1 = new TransactionModel()
            {
                ReferenceValue = "1",
                Date = DateTime.Today.AddDays(-5),
                OriginalPayeeName = "this",
            };

            var trans2 = new TransactionModel()
            {
                ReferenceValue = "2",
                OriginalPayeeName = "that",
                Date = DateTime.Today.AddDays(-5)
            };

            var trans3 = new TransactionModel()
            {
                ReferenceValue = "3",
                OriginalPayeeName = "other",
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

        private Mock<ITransactionDefaults> MakeTransDefaults()
        {
            var thisPayee = new PayeeModel()
            {
                Id = 1,
                Name = "This Thing",
                ImportNames = new List<ImportNameToPayeeModel>()
                {
                    new ImportNameToPayeeModel()
                    {
                    Id = 1,
                    PayeeId = 1,
                    ImportName = "this"
                    }
                },
                DefaultDetails = new List<PayeeDefaultDetailsModel>()
                {
                    new PayeeDefaultDetailsModel()
                    {
                        Id = 1,
                        PayeeId = 1
                    }
                }
            };

            var thatPayee = new PayeeModel()
            {
                Id = 2,
                Name = "That Thing",
                ImportNames = new List<ImportNameToPayeeModel>()
                {
                    new ImportNameToPayeeModel()
                    {
                    Id = 2,
                    PayeeId = 2,
                    ImportName = "that"
                    }
                },
                DefaultDetails = new List<PayeeDefaultDetailsModel>()
                {
                    new PayeeDefaultDetailsModel()
                    {
                        Id = 2,
                        PayeeId = 2
                    },
                    new PayeeDefaultDetailsModel()
                    {
                        Id = 3,
                        PayeeId = 2
                    }
                }
            };

            var thisDetails = new List<TransactionDetailModel>()
            {
                new TransactionDetailModel()
            };

            var thatDetails = new List<TransactionDetailModel>()
            {
                new TransactionDetailModel(),
                new TransactionDetailModel()
            };

            var defaults = new Mock<ITransactionDefaults>();
            defaults.Setup(x => x.GetDefaultPayees("this")).Returns(new List<PayeeModel>() { thisPayee });
            defaults.Setup(x => x.GetDefaultPayees("that")).Returns(new List<PayeeModel>() { thatPayee });
            defaults.Setup(x => x.GetDefaultPayees("other")).Returns(new List<PayeeModel>() {  });
            defaults.Setup(x => x.GetDefaultPayeeDetails(thisPayee, It.IsAny<TransactionModel>())).Returns(thisDetails);
            defaults.Setup(x => x.GetDefaultPayeeDetails(thatPayee, It.IsAny<TransactionModel>())).Returns(thatDetails);
            return defaults;
        }
    }
}
