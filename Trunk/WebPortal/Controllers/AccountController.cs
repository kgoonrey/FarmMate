using WebPortal.Data;
using WebPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace WebPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(SignInManager<ApplicationUser> signInManager)
        {
            this.signInManager = signInManager;
        }

        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                if(model.ConfirmPassword != null && model.ConfirmPassword != string.Empty)
                {
                    var user = signInManager.UserManager.Users.FirstOrDefault(x => x.UserName == model.UserName);
                    var checkPassword = signInManager.UserManager.CheckPasswordAsync(user, string.Empty);
                    if (!checkPassword.Result)
                    {
                        ModelState.AddModelError(string.Empty, "Error Try Again.");
                        return View(model);
                    }
                    if (model.Password != model.ConfirmPassword)
                    {
                        ModelState.AddModelError(string.Empty, "Passwords Don't Match.");
                        return View(model);
                    }
                    user.PasswordHash = signInManager.UserManager.PasswordHasher.HashPassword(user, model.Password);
                    var passwordChange = await signInManager.UserManager.UpdateAsync(user);

                    if (!passwordChange.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(model);
                    }
                }
                
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {                   
                    return RedirectToLocal(returnUrl);
                }               
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }           
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOff()
        {
            await signInManager.SignOutAsync();           
            return RedirectToAction("Login");
        }

       public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Manage()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [HttpPost]
        [Route("api/Data/GetUserDetails")]
        public JsonResult GetUserDetails()
        {
            using (var context = new DataModel())
            {
                var user = GetUser();
                var userDetailRow = context.AspNetUsers.FirstOrDefault(x => x.Id == user.Result.Id);
                return Json(userDetailRow);
            }
        }

        [HttpPost]
        [Route("api/Data/UpdatePassword")]
        public JsonResult UpdatePassword([FromBody]ChangePassword changePassword)
        {
            var signedInuser = GetUser();
            var user = signInManager.UserManager.Users.FirstOrDefault(x => x.Id == signedInuser.Result.Id);
            var checkPassword = signInManager.UserManager.CheckPasswordAsync(user, changePassword.OldPassword);
            if (!checkPassword.Result)
            {
                ModelState.AddModelError(string.Empty, "Password Incorrect.");
                return Json("Password Incorrect");
            }

            user.PasswordHash = signInManager.UserManager.PasswordHasher.HashPassword(user, changePassword.NewPassword);
            var passwordChange = signInManager.UserManager.UpdateAsync(user);

            if (!passwordChange.Result.Succeeded)
            {
                return Json("Password Update Failed");
            }

            return Json("Password Updated");
        }

        [HttpPost]
        [Route("api/Data/UpdateDetails")]
        public JsonResult UpdateDetails([FromBody]UpdateDetails details)
        {
            using (var context = new DataModel())
            {
                var signedInuser = GetUser();
                var userDetailRow = context.AspNetUsers.FirstOrDefault(x => x.Id == signedInuser.Result.Id);
                if (userDetailRow == null)
                    return Json(false);

                userDetailRow.Name = details.Name;
                userDetailRow.Email = details.Email;
                context.SaveChanges();

                return Json(true);
            }
        }

        [HttpPost]
        [Route("api/Data/GetPasswordReset")]
        public JsonResult GetPasswordReset([FromBody]string username)
        {
            using (var context = new DataModel())
            {
                var userDetailRow = context.AspNetUsers.FirstOrDefault(x => x.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (userDetailRow == null || userDetailRow.PasswordHash != string.Empty)
                    return Json(false);

                return Json(true);
            }
        }

        private async Task<ApplicationUser> GetUser()
        {
            return await signInManager.UserManager.GetUserAsync(HttpContext.User);
        }
    }
}
