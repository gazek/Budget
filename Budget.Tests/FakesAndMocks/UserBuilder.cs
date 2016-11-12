using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Budget.API.Tests.Fakes
{
    public static class UserBuilder
    {
        public static IPrincipal CreateUser()
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
