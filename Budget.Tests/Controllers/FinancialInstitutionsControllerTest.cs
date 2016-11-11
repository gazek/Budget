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
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Web.Http.Routing;
using System.Linq;
using System.Linq.Expressions;

namespace Budget.API.Tests.Controllers
{
    [TestClass]
    public class FinancialInstitutionsControllerTest
    {

        [TestMethod]
        public void FICreateWithValidModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);
            var urlHelperMock = new Moq.Mock<UrlHelper>();
            urlHelperMock.Setup(x => x.Link(Moq.It.IsAny<string>(), Moq.It.IsAny<Object>()))
                .Returns("anyvalue");
            controller.Url = urlHelperMock.Object;

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(CreatedNegotiatedContentResult<FinancialInstitutionViewModel>));
        }

        [TestMethod]
        public void FICreateWithInvalidModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);
            controller.ModelState.AddModelError("test", "TEST");

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

            // Assert
            Assert.IsNotInstanceOfType(result, typeof(OkNegotiatedContentResult<FinancialInstitutionViewModel>));
        }

        [TestMethod]
        public void FICreateWithDuplicateModel()
        {
            // Arrange
            var contextMock = GetContextMock();
            var inner2 = FormatterServices.GetUninitializedObject(typeof(SqlException)) as SqlException;
            var inner1 = new Exception("", inner2);
            contextMock.Setup(x => x.SaveChanges()).Throws(new DbUpdateException("", inner1));
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);

            // Act
            IHttpActionResult result = controller.Create(GetValidBindingModel());

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void FIGetExistsAndAuthorized()
        {
            // Arrange
            var contextMock = GetContextMock();
            var userId = contextMock.Object.FinancialInstitutions.Add(new FinancialInstitutionModel()).UserId;
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);

            // Act
            IHttpActionResult result = controller.Get(1);

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void FIGetExistsAndUnauthorized()
        {
            // Arrange
            var contextMock = GetContextMock();
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);
            controller.User = CreateUser();

            // Act
            IHttpActionResult result = controller.Get(1);

            // Assert
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void FIGetDoesNotExist()
        {
            // Arrange
            var contextMock = GetContextMock();
            FinancialInstitutionsController controller = new FinancialInstitutionsController(contextMock.Object);

            // Act
            IHttpActionResult result = controller.Get(1);

            // Assert
            Assert.IsTrue(false);
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

        private Moq.Mock<IApplicationDbContext> GetContextMock()
        {
            var entityWithId = ModelMapper.BindingToEntity(GetValidBindingModel(), CreateUser());
            entityWithId.Id = 1;
            var contextMock = new Moq.Mock<IApplicationDbContext>();
            var fiMock = new Moq.Mock<DbSet<FinancialInstitutionModel>>();
            var iqueryFiMock = new Moq.Mock<IQueryable<FinancialInstitutionModel>>();
            //iqueryFiMock.Setup(x => x.FirstOrDefault()).Returns(entityWithId);
            //fiMock.Setup(x => x.Where(Moq.It.IsAny<Expression<Func<FinancialInstitutionModel, bool>>>()))
            //    .Returns(iqueryFiMock.Object);
            fiMock.Setup(x => x.Add(Moq.It.IsAny<FinancialInstitutionModel>())).Returns(entityWithId);
            contextMock.SetupGet(x => x.FinancialInstitutions).Returns(fiMock.Object);
            contextMock.Setup(x => x.SaveChanges()).Returns(1);
            return contextMock;
        }
    }
}

