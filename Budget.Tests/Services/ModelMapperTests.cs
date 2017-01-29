using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Budget.DAL.Models;
using Budget.API.Models;
using Budget.API.Services;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using Budget.API.Tests.Fakes;
using System.Linq;
using Budget.API.Services.OFXClient;

namespace Budget.Tests.Services
{
    [TestClass]
    public class ModelMapperTests
    {
        private IPrincipal _user;

        public ModelMapperTests()
        {
            _user = UserBuilder.CreateUser();
        }

        [TestMethod]
        public void FiBindingToEntityRequiredFieldsOnly()
        {
            // Arrange
            var model = new FinancialInstitutionCreateBindingModel
            {
                Name = "Some FI Name",
                OfxFid = 1234,
                OfxUrl = "Https://ofx.bank.com",
                OfxOrg = "MYORGNAME",
                Username = "someusername",
                Password = "highlysecurepassword",
                ConfirmPassword = "highlysecurepassword",
            };

            // Act
            var result = ModelMapper.BindingToEntity(model, _user);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FinancialInstitutionModel));
            Assert.AreEqual(0, result.Id);
            Assert.AreEqual(model.Name, result.NameStylized);
            Assert.AreEqual(model.OfxFid, result.OfxFid);
            Assert.AreEqual(model.OfxUrl, result.OfxUrl);
            Assert.AreEqual(model.OfxOrg, result.OfxOrg);
            Assert.AreEqual(_user.Identity.GetUserId(), result.UserId);
            Assert.AreEqual(model.Username, result.Username);
            Assert.AreNotEqual(model.Password, result.PasswordHash);
            Assert.AreNotEqual("", result.PasswordHash);
            Assert.IsNotNull(result.PasswordHash);
            Assert.IsNull(result.CLIENTUID);
        }

        [TestMethod]
        public void FiBindingToEntityWithOptionalFields()
        {
            // Arrange
            var model = new FinancialInstitutionCreateBindingModel
            {
                Name = "Some FI Name",
                OfxFid = 5678,
                OfxUrl = "Https://ofx.bank.com",
                OfxOrg = "MYORGNAME",
                Username = "someusername",
                Password = "highlysecurepassword",
                ConfirmPassword = "highlysecurepassword",
                CLIENTUID = Guid.NewGuid().ToString()
            };

            // Act
            var result = ModelMapper.BindingToEntity(model, _user);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FinancialInstitutionModel));
            Assert.AreEqual(model.Name, result.NameStylized);
            Assert.AreEqual(model.OfxFid, result.OfxFid);
            Assert.AreEqual(model.OfxUrl, result.OfxUrl);
            Assert.AreEqual(model.OfxOrg, result.OfxOrg);
            Assert.AreEqual(_user.Identity.GetUserId(), result.UserId);
            Assert.AreEqual(model.Username, result.Username);
            Assert.AreNotEqual(model.Password, result.PasswordHash);
            Assert.AreNotEqual("", result.PasswordHash);
            Assert.IsNotNull(result.PasswordHash);
            Assert.AreEqual(model.CLIENTUID, result.CLIENTUID);
        }

        [TestMethod]
        public void FiEntityToViewRequiredFieldsOnly()
        {
            // Arrange
            var model = new FinancialInstitutionModel
            {
                Id = 1234,
                Name = "Some FI Name",
                OfxFid = 5678,
                OfxUrl = "Https://ofx.bank.com",
                OfxOrg = "MYORGNAME",
                UserId = "SomeName",
                PasswordHash = new byte[] { 0x20, 0x20 },
                User = new ApplicationUser()
            };

            // Act
            var result = ModelMapper.EntityToView(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FinancialInstitutionViewModel));
            Assert.AreEqual(model.Id, result.Id);
            Assert.AreEqual(model.NameStylized, result.Name);
            Assert.AreEqual(model.OfxFid, result.OfxFid);
            Assert.AreEqual(model.OfxUrl, result.OfxUrl);
            Assert.AreEqual(model.OfxOrg, result.OfxOrg);
            Assert.AreEqual(model.Username, result.Username);
            Assert.AreEqual(string.Empty, result.CLIENTUID);
        }

        [TestMethod]
        public void FiEntityToViewWithOptionalFields()
        {
            // Arrange
            var model = new FinancialInstitutionModel
            {
                Id = 1234,
                Name = "Some FI Name",
                OfxFid = 5678,
                OfxUrl = "Https://ofx.bank.com",
                OfxOrg = "MYORGNAME",
                UserId = "SomeName",
                PasswordHash = new byte[] { 0x20, 0x20 },
                CLIENTUID = Guid.NewGuid().ToString(),
                User = new ApplicationUser()
            };

            // Act
            var result = ModelMapper.EntityToView(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FinancialInstitutionViewModel));
            Assert.AreEqual(model.Id, result.Id);
            Assert.AreEqual(model.NameStylized, result.Name);
            Assert.AreEqual(model.OfxFid, result.OfxFid);
            Assert.AreEqual(model.OfxUrl, result.OfxUrl);
            Assert.AreEqual(model.OfxOrg, result.OfxOrg);
            Assert.AreEqual(model.Username, result.Username);
            Assert.AreEqual(model.CLIENTUID, result.CLIENTUID);
        }

        [TestMethod]
        public void AccountEntityToView()
        {
            // Arrange
            var model = new AccountModel
            {
                Id = 1223,
                FinancialInstitution = new FinancialInstitutionModel()
                {
                    UserId = Guid.NewGuid().ToString()
                },
                FinancialInstitutionId = 12,
                RoutingNumber = 1234567890,
                Number = "s123",
                Name = "my account",
                Type = AccountType.Savings,
                Description = "a savings account",
                Transactions = new List<TransactionModel>(),
                BalanceHistory = new List<BalanceModel>()
            };

            // Act
            var result = ModelMapper.EntityToView(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(AccountViewModel));
            Assert.AreEqual(model.Id, result.Id);
            Assert.AreEqual(model.FinancialInstitutionId, result.FinancialInstitutionId);
            Assert.AreEqual(model.RoutingNumber, result.RoutingNumber);
            Assert.AreEqual(model.Number, result.Number);
            Assert.AreEqual(model.Name, result.Name);
            Assert.AreEqual(model.Type, result.Type);
            Assert.AreEqual(model.Description, result.Description);
            Assert.AreEqual(model.BalanceHistory.OrderByDescending(x => x.AsOfDate).FirstOrDefault(), result.Balance);
        }

        [TestMethod]
        public void AccountEntitiesToListView()
        {
            // Arrange
            var model = new AccountModel
            {
                Id = 1223,
                FinancialInstitution = new FinancialInstitutionModel()
                {
                    Id = 12,
                    UserId = Guid.NewGuid().ToString()
                },
                FinancialInstitutionId = 12,
                RoutingNumber = 1234567890,
                Number = "s123",
                Name = "my account",
                Type = AccountType.Savings,
                Description = "a savings account",
                Transactions = new List<TransactionModel>(),
                BalanceHistory = new List<BalanceModel>()
            };

            // Act
            var result = ModelMapper.EntityToListView(model, model.FinancialInstitutionId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(AccountListViewModel));
            Assert.AreEqual(model.FinancialInstitutionId, result.FinancialInstitutionId);
            Assert.AreEqual(model.RoutingNumber, result.RoutingNumber);
            Assert.AreEqual(model.Number, result.Number);
            Assert.AreEqual(model.Name, result.Name);
            Assert.AreEqual(model.Type, result.Type);
        }

        [TestMethod]
        public void BalanceEntityToView()
        {
            // Arrange
            var model = new BalanceModel()
            {
                Id = 1,
                AccountId = 2,
                AsOfDate = DateTime.Today,
                Amount = 123.45M
            };

            // Act
            var result = ModelMapper.EntityToView(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BalanceViewModel));
            Assert.AreEqual(model.Id, result.Id);
            Assert.AreEqual(model.AccountId, result.AccountId);
            Assert.AreEqual(model.AsOfDate, result.AsOfDate);
            Assert.AreEqual(model.Amount, result.Amount);
        }

        [TestMethod]
        public void AccountTypeMapEntityToOFXSavings()
        {
            // Arrange
            var entity = AccountType.Savings;

            // Act
            var result = ModelMapper.Type(entity);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OFXRequestConfigAccountType));
            Assert.AreEqual(result, OFXRequestConfigAccountType.SAVINGS);
        }

        [TestMethod]
        public void AccountTypeMapEntityToOFXChecking()
        {
            // Arrange
            var entity = AccountType.Checking;

            // Act
            var result = ModelMapper.Type(entity);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OFXRequestConfigAccountType));
            Assert.AreEqual(result, OFXRequestConfigAccountType.CHECKING);
        }

        [TestMethod]
        public void AccountTypeMapEntityToOFXCreditCard()
        {
            // Arrange
            var entity = AccountType.CreditCard;

            // Act
            var result = ModelMapper.Type(entity);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OFXRequestConfigAccountType));
            Assert.AreEqual(result, OFXRequestConfigAccountType.CREDITCARD);
        }

        [TestMethod]
        public void AccountTypeMapOFXToEntitySavings()
        {
            // Arrange
            var entity = OFXRequestConfigAccountType.SAVINGS;

            // Act
            var result = ModelMapper.Type(entity);

            // Assert
            Assert.IsInstanceOfType(result, typeof(AccountType));
            Assert.AreEqual(result, AccountType.Savings);
        }

        [TestMethod]
        public void AccountTypeMapOFXToEntityChecking()
        {
            // Arrange
            var entity = OFXRequestConfigAccountType.CHECKING;

            // Act
            var result = ModelMapper.Type(entity);

            // Assert
            Assert.IsInstanceOfType(result, typeof(AccountType));
            Assert.AreEqual(result, AccountType.Checking);
        }

        [TestMethod]
        public void AccountTypeMapOFXToEntityCreditCard()
        {
            // Arrange
            var entity = OFXRequestConfigAccountType.CREDITCARD;

            // Act
            var result = ModelMapper.Type(entity);

            // Assert
            Assert.IsInstanceOfType(result, typeof(AccountType));
            Assert.AreEqual(result, AccountType.CreditCard);
        }

        [TestMethod]
        public void NEED_AccountModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_BalanceModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_FIModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_TransactionModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_TransDetailModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_CategoryModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_SubCatModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_PayeeModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_PayeeDetailModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NEED_PayeeImportNameModelGetUserId()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsTrue(false);
        }
    }
}
