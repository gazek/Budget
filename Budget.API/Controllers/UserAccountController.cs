using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Budget.API.Models;
using Budget.DAL.Models;
using Budget.DAL;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;

namespace Budget.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/UserAccount")]
    public class UserAccountController : ApiController
    {
        private ApplicationUserManager _userManager;

        public UserAccountController()
        {
        }
        
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // POST api/UserAccount/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Username, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            IHttpActionResult response = AddDefaultPayeeAndCategories(user);

            return response ?? Ok();
        }

        // GET api/UserAccount/UserInfo
        [Route("UserInfo")]
        public UserInfoViewModel GetUserInfo()
        {
            return new UserInfoViewModel
            {
                Username = User.Identity.GetUserName(),
                Email = UserManager.GetEmail(User.Identity.GetUserId())
            };
        }

        // POST api/UserAccount/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/UserAccount/RemoveLogin
        [Route("RemoveLogin")]
        public async Task<IHttpActionResult> RemoveLogin()
        {
            IdentityResult result;

            result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/UserAccount/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        #endregion

        private IHttpActionResult AddDefaultPayeeAndCategories(ApplicationUser user)
        {
            // create default category and payee
            PayeeModel unAssigned = new PayeeModel()
            {
                Name = "unassigned",
                NameStylized = "Unassigned",
                UserId = user.Id
            };
            CategoryModel uncat = new CategoryModel()
            {
                Name = "uncategorized",
                NameStylized = "Uncategorized",
                SubCategories = new List<SubCategoryModel>()
                {
                    new SubCategoryModel()
                    {
                        Name = "uncategorized",
                        NameStylized = "Uncategorized",
                    }
                },
                UserId = user.Id
            };
            using (ApplicationDbContext dbContext = new ApplicationDbContext())
            {
                dbContext.Payees.Add(unAssigned);
                dbContext.Categories.Add(uncat);
                try
                {
                    dbContext.SaveChanges();
                }
                catch
                {
                    return BadRequest("Failed to create default payee and categories");
                }
            }
            return null;
        }
    }
}
