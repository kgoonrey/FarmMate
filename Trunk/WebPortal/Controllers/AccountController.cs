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

        [Authorize]
        public IActionResult Manage()
        {
            return View();
        }

        public IActionResult SendPasswordResetLink(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult SendPasswordResetLink(ResetPassword model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                if (model.UserName == null || model.UserName == string.Empty)
                {
                    ModelState.AddModelError(string.Empty, "Please enter a valid username");
                    return View(model);
                }

                var user = signInManager.UserManager.FindByNameAsync(model.UserName).Result;

                if (user == null || !user.EmailConfirmed || user.Email == null || user.Email == string.Empty)
                {
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                var token = signInManager.UserManager.GeneratePasswordResetTokenAsync(user).Result;

                var resetLink = Url.Action("ResetPassword",
                                "Account", new { token = token },
                                 protocol: HttpContext.Request.Scheme);

                using (var context = new DataModel())
                {
                    var tradingEntity = context.TradingEntity.FirstOrDefault();
                    Tools.SendEmail(tradingEntity, user.Email, "Password Reset", $"Please click link to reset password for Farm Mate{Environment.NewLine}{Environment.NewLine}{resetLink}");

                }

                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return View(model);

        }

        public IActionResult ResetPassword(string token)
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(PasswordResetViewModel model)
        {
            if(model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords Don't Match.");
                return View(model);
            }

            var user = signInManager.UserManager.FindByNameAsync(model.UserName).Result;

            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "Error while resetting the password!");
                return View(model);
            }

            IdentityResult result = signInManager.UserManager.ResetPasswordAsync(user, model.Token, model.Password).Result;
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            else
            {
                var shortPassword = result.Errors.FirstOrDefault(x => x.Code == "PasswordTooShort");
                if (shortPassword != null)
                    ModelState.AddModelError(string.Empty, shortPassword.Description);
                else
                    ModelState.AddModelError(string.Empty, "Error while resetting the password!");
                return View(model);
            }
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

        private async Task<ApplicationUser> GetUser()
        {
            return await signInManager.UserManager.GetUserAsync(HttpContext.User);
        }
    }
}
