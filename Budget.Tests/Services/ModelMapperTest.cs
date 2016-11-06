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
    /// <summary>
    /// Summary description for ModelMapperTests
    /// </summary>
    [TestClass]
    public class ModelMapperTest
    {
        private IPrincipal _user;

        public ModelMapperTest()
        {
            var userMock = new Moq.Mock<IPrincipal>();

            string userId = Guid.NewGuid().ToString();

            List<Claim> claims = new List<Claim>
            {
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId)
            };

            var testIdentity = new ClaimsIdentity(claims);

            var identityMock = new Moq.Mock<IIdentity>();
            //identityMock.SetupGet(x => x.Name).Returns("applicationUserName");
            //userMock.SetupGet(x => x.Identity).Returns(identityMock.Object);
            userMock.SetupGet(x => x.Identity).Returns(testIdentity);
            _user = userMock.Object;
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void FiBindingToEntityRequiredFieldsOnly()
        {
            // Arrange
            var model = new FinancialInstitutionBindingModel
            {
                Name = "Some FI Name",
                OfxFid = 1234,
                OfxUrl = "Https://ofx.bank.com",
                OfxOrg = "MYORGNAME",
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
            Assert.IsNull(result.Username);
            Assert.IsNull(result.PasswordHash);
            Assert.IsNull(result.CLIENTUID);
        }

        [TestMethod]
        public void FiBindingToEntityWithOptionalFields()
        {
            // Arrange
            var model = new FinancialInstitutionBindingModel
            {
                Id = 1234,
                Name = "Some FI Name",
                OfxFid = 5678,
                OfxUrl = "Https://ofx.bank.com",
                OfxOrg = "MYORGNAME",
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
            Assert.IsNull(result.Username);
            Assert.IsNull(result.PasswordHash);
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
                PasswordHash = Guid.NewGuid().ToString(),
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
                PasswordHash = Guid.NewGuid().ToString(),
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
