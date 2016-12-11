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
    public class TransactionDefaultsTests
    {
        [TestMethod]
        public void GetDefaultStatus()
        {
            // Arrange
            var defaults = new TransactionDefaults(new MockDbContext().GetMock());

            // Act
            var result = defaults.GetDefaultStatus();

            // Assert
            Assert.IsInstanceOfType(result, typeof(TransactionStatus));
            Assert.AreEqual(TransactionStatus.New, result);
        }

        [TestMethod]
        public void GetDefaultDateAdded()
        {
            // Arrange
            var defaults = new TransactionDefaults(new MockDbContext().GetMock());

            // Act
            var result = defaults.GetDefaultDateAdded();

            // Assert
            Assert.IsInstanceOfType(result, typeof(DateTime));
            Assert.AreEqual(DateTime.Now.Date, result.Date);
        }

        [TestMethod]
        public void GetDefaultLastEditDate()
        {
            // Arrange
            var defaults = new TransactionDefaults(new MockDbContext().GetMock());

            // Act
            var result = defaults.GetDefaultLastEditDate();

            // Assert
            Assert.IsInstanceOfType(result, typeof(DateTime));
            Assert.AreEqual(DateTime.Now.Date, result.Date);
        }

        [TestMethod]
        public void GetDefaultPayeesSingleMatch()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = MakeContext(user);
            var inputPayeeName = "FooBar";
            var defaults = new TransactionDefaults(contextMock.Object);

            // Act
            var result = defaults.GetDefaultPayees(user.Identity.GetUserId(), inputPayeeName);

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetDefaultPayeesSingleMatchCaseInsensitive()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = MakeContext(user);
            var inputPayeeName = "OTHER PAYEE";
            var defaults = new TransactionDefaults(contextMock.Object);

            // Act
            var result = defaults.GetDefaultPayees(user.Identity.GetUserId(), inputPayeeName);

            // Assert
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetDefaultPayeesMultipleMatch()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = MakeContext(user);
            var inputPayeeName = "FooBar2";
            var defaults = new TransactionDefaults(contextMock.Object);

            // Act
            var result = defaults.GetDefaultPayees(user.Identity.GetUserId(), inputPayeeName);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetDefaultPayeesNoMatch()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = MakeContext(user);
            var inputPayeeName = "nothing should match this";
            var defaults = new TransactionDefaults(contextMock.Object);

            // Act
            var result = defaults.GetDefaultPayees(user.Identity.GetUserId(), inputPayeeName);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetDefaultPayeeDetailsWithOnePayeeNoTransaction()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = MakeContext(user);
            var defaults = new TransactionDefaults(contextMock.Object);
            var payee = contextMock.Object.Payees.Where(x => x.Name.Equals("otherone")).First();

            // Act
            var result = defaults.GetDefaultPayeeDetails(payee);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(22, result[0].CategoryId);
            Assert.AreEqual(20, result[0].SubCategoryId);
        }

        [TestMethod]
        public void GetDefaultPayeeDetailsWithManyPayeesNoTransaction()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = MakeContext(user);
            var defaults = new TransactionDefaults(contextMock.Object);
            var payee = contextMock.Object.Payees.Where(x => x.Name.Equals("FooBar2")).First();

            // Act
            var result = defaults.GetDefaultPayeeDetails(payee);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void GetTopPayeeNameNone()
        {
            // Arrange
            var payeeName = "no details";
            var user = UserBuilder.CreateUser();
            var contextMock = MakeContext(user);
            var defaults = new TransactionDefaults(contextMock.Object);
            var payee = contextMock.Object.Payees.Where(x => x.Name.Equals(payeeName)).First();
            var details = defaults.GetDefaultPayeeDetails(payee);
            var transaction = new TransactionModel()
            {
                Details = details
            };
            // Act
            var result = defaults.GetTopPayeeName(transaction);

            // Assert
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void GetTopPayeeNameSinglePayee()
        {
            // Arrange
            var payeeName = "otherone";
            var user = UserBuilder.CreateUser();
            var contextMock = MakeContext(user);
            var defaults = new TransactionDefaults(contextMock.Object);
            var payee = contextMock.Object.Payees.Where(x => x.Name.Equals(payeeName)).First();
            var details = defaults.GetDefaultPayeeDetails(payee);
            var transaction = new TransactionModel()
            {
                Details = details
            };
            // Act
            var result = defaults.GetTopPayeeName(transaction);

            // Assert
            Assert.AreEqual(payeeName, result);
        }

        [TestMethod]
        public void GetTopPayeeNameMultiplePayees()
        {
            // Arrange
            var user = UserBuilder.CreateUser();
            var contextMock = MakeContext(user);
            var defaults = new TransactionDefaults(contextMock.Object);
            var payees = contextMock.Object.Payees
                .Where(x => x.Name.Contains("Foo"))
                .Where(x => x.UserId.Equals(user.Identity.GetUserId()))
                .ToList();
            var details = defaults.GetDefaultPayeeDetails(payees[0]);
            details.AddRange(defaults.GetDefaultPayeeDetails(payees[1]));
            var transaction = new TransactionModel()
            {
                Details = details
            };
            // Act
            var result = defaults.GetTopPayeeName(transaction);

            // Assert
            Assert.AreEqual("Multiple", result);
        }

        private Mock<IApplicationDbContext> MakeContext(IPrincipal user)
        {
            var detail1 = new PayeeDefaultDetailsModel()
            {
                PayeeId = 1,
                CategoryId = 11,
                SubCategoryId = 10,
                Allocation = 100
            };
            var detail2 = new PayeeDefaultDetailsModel()
            {
                PayeeId = 2,
                CategoryId = 22,
                SubCategoryId = 20,
                Allocation = 100
            };
            var detail41 = new PayeeDefaultDetailsModel()
            {
                PayeeId = 4,
                CategoryId = 44,
                SubCategoryId = 40,
                Allocation = 75
            };
            var detail42 = new PayeeDefaultDetailsModel()
            {
                PayeeId = 4,
                CategoryId = 44,
                SubCategoryId = 41,
                Allocation = 25,
            };
            var payee1 = new PayeeModel()
            {
                Id = 1,
                UserId = user.Identity.GetUserId(),
                Name = "Foo Bar",
                ImportNames = new List<ImportNameToPayeeModel>()
                {
                    new ImportNameToPayeeModel()
                    {
                        PayeeId = 1,
                        ImportName = "FooBar"
                    },
                    new ImportNameToPayeeModel()
                    {
                        PayeeId = 1,
                        ImportName = "Foo Bar"
                    }
                },
                DefaultDetails = new List<PayeeDefaultDetailsModel>() { detail1 }
            };
            var payee2 = new PayeeModel()
            {
                Id = 2,
                UserId = user.Identity.GetUserId(),
                Name = "otherone",
                ImportNames = new List<ImportNameToPayeeModel>()
                {
                    new ImportNameToPayeeModel()
                    {
                        PayeeId = 1,
                        ImportName = "other payee"
                    },
                    new ImportNameToPayeeModel()
                    {
                        PayeeId = 1,
                        ImportName = "other1"
                    }
                },
                DefaultDetails = new List<PayeeDefaultDetailsModel>() { detail2 }
            };
            var payee3 = new PayeeModel()
            {
                Id = 3,
                UserId = Guid.NewGuid().ToString(),
                Name = "Foo Bar",
                ImportNames = new List<ImportNameToPayeeModel>()
                {
                    new ImportNameToPayeeModel()
                    {
                        PayeeId = 1,
                        ImportName = "FooBar"
                    },
                    new ImportNameToPayeeModel()
                    {
                        PayeeId = 1,
                        ImportName = "Foo Bar"
                    }
                }
            };
            var payee4 = new PayeeModel()
            {
                Id = 3,
                UserId = user.Identity.GetUserId(),
                Name = "FooBar2",
                ImportNames = new List<ImportNameToPayeeModel>()
                {
                    new ImportNameToPayeeModel()
                    {
                        PayeeId = 1,
                        ImportName = "Foobar2"
                    }
                },
                DefaultDetails = new List<PayeeDefaultDetailsModel>() { detail41, detail42 }
            };
            var payee5 = new PayeeModel()
            {
                Id = 3,
                UserId = user.Identity.GetUserId(),
                Name = "no details",
                ImportNames = new List<ImportNameToPayeeModel>()
                {
                    new ImportNameToPayeeModel()
                    {
                        PayeeId = 1,
                        ImportName = "no details"
                    }
                },
                DefaultDetails = new List<PayeeDefaultDetailsModel>()
            };
            var payees = new List<PayeeModel>() { payee1, payee2, payee3, payee4, payee5};
            // mock context
            var contextMockBuilder = new MockDbContext()
                .WithData(payees)
                .SetupFind(1, payee1)
                .SetupFind(2, payee2)
                .SetupFind(3, payee3)
                .SetupFind(4, payee4)
                .SetupFind(5, payee5)
                .Finalize();

            return contextMockBuilder.Context;
        }
    }
}
