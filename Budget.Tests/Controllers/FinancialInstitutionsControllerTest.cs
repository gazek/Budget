using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Budget.API.Controllers;
using Budget.DAL;
using Budget.API.Models;
using Budget.API.Services;
using System.Security.Principal;
using System.Collections.Generic;
using System.Security.Claims;
using System.Data.Entity;
using Budget.DAL.Models;
using System.Web.Http.Results;
using System.Web.Http;
using System.Data.Entity.Infrastructure;

namespace Budget.API.Tests.Controllers
{
    [TestClass]
    public class FinancialInstitutionsControllerTest
    {

        [TestMethod]
        public void FICreateWithValidModel()
        {
            // Arrange
            var userMock = CreateUser();
            var bindingModel = GetValidBindingModel();
            var entity = ModelMapper.BindingToEntity(bindingModel, userMock);
            var entityWithId = ModelMapper.BindingToEntity(bindingModel, userMock);
            var viewModel = ModelMapper.EntityToView(entity);
            var contextMock = new Moq.Mock<IApplicationDbContext>();
            var fiMock = new Moq.Mock<DbSet<FinancialInstitutionModel>>();
            fiMock.Setup(x => x.Add(Moq.It.IsAny< FinancialInstitutionModel>())).Returns(entityWithId);
            contextMock.SetupGet(x => x.FinancialInstitutions).Returns(fiMock.Object);
            contextMock.Setup(x => x.SaveChanges()).Returns(1);
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);

            // Act
            IHttpActionResult result = controller.Create(bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<FinancialInstitutionViewModel>));
        }

        [TestMethod]
        public void FICreateWithInvalidModel()
        {
            // Arrange
            var userMock = CreateUser();
            var bindingModel = GetValidBindingModel();
            var contextMock = new Moq.Mock<IApplicationDbContext>();
            
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);
            controller.ModelState.AddModelError("test", "TEST");

            // Act
            IHttpActionResult result = controller.Create(bindingModel);

            // Assert
            Assert.IsNotInstanceOfType(result, typeof(OkNegotiatedContentResult<FinancialInstitutionViewModel>));
        }

        [TestMethod]
        public void FICreateWithDuplicateModel()
        {
            // Arrange
            var userMock = CreateUser();
            var bindingModel = GetValidBindingModel();
            var entity = ModelMapper.BindingToEntity(bindingModel, userMock);
            var entityWithId = ModelMapper.BindingToEntity(bindingModel, userMock);
            var viewModel = ModelMapper.EntityToView(entity);
            var contextMock = new Moq.Mock<IApplicationDbContext>();
            var fiMock = new Moq.Mock<DbSet<FinancialInstitutionModel>>();
            fiMock.Setup(x => x.Add(Moq.It.IsAny<FinancialInstitutionModel>())).Returns(entityWithId);
            contextMock.SetupGet(x => x.FinancialInstitutions).Returns(fiMock.Object);
            contextMock.Setup(x => x.SaveChanges()).Throws<DbUpdateException>();
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);

            // Act
            IHttpActionResult result = controller.Create(bindingModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        private FinancialInstitutionCreateBindingModel GetValidBindingModel()
        {
            return new FinancialInstitutionCreateBindingModel()
            {
                Name = "My new FI",
                OfxFid = 9876,
                OfxOrg = "FI Org Name",
                OfxUrl = "https://ofx.bank.com",
                Username = "myUsername",
                Password = "mySuperSecurePassword",
                ConfirmPassword = "mySuperSecurePassword"
            };
        }

        private IPrincipal CreateUser()
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
            return userMock.Object;
        }
    }
}

