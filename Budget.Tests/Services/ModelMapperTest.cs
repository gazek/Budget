using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Budget.DAL.Models;
using Budget.API.Models;
using Budget.API.Services;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Security.Claims;

namespace Budget.Tests.Services
{
    [TestClass]
    public class ModelMapperTest
    {
        private IPrincipal _user;

        public ModelMapperTest()
        {
            // create user mock
            var userMock = new Moq.Mock<IPrincipal>();

            // Create a fake Identity
            // Cannot use Moq since GetUserId() is an extension method
            string userId = Guid.NewGuid().ToString();
            List<Claim> claims = new List<Claim>
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId)
            };
            var identityMock = new ClaimsIdentity(claims);
            userMock.SetupGet(x => x.Identity).Returns(identityMock);

            // assign to field
            _user = userMock.Object;
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
            Assert.AreEqual(model.Name, result.Name);
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
                Id = 1234,
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
            Assert.AreEqual(model.Id, result.Id);
            Assert.AreEqual(model.Name, result.Name);
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
            Assert.AreEqual(model.Name, result.Name);
            Assert.AreEqual(model.OfxFid, result.OfxFid);
            Assert.AreEqual(model.OfxUrl, result.OfxUrl);
            Assert.AreEqual(model.OfxOrg, result.OfxOrg);
            Assert.AreEqual(model.Username, result.Username);
            Assert.IsNull(result.CLIENTUID);
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
            Assert.AreEqual(model.Name, result.Name);
            Assert.AreEqual(model.OfxFid, result.OfxFid);
            Assert.AreEqual(model.OfxUrl, result.OfxUrl);
            Assert.AreEqual(model.OfxOrg, result.OfxOrg);
            Assert.AreEqual(model.Username, result.Username);
            Assert.AreEqual(model.CLIENTUID, result.CLIENTUID);
        }
    }
}
