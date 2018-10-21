using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebPortal.Data;
using WebPortal.Models;

namespace WebPortal.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Index()
        {
            List<UserListViewModel> model = new List<UserListViewModel>();
            model = userManager.Users.Select(u => new UserListViewModel
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            }).ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            UserViewModel model = new UserViewModel();
            model.ApplicationRoles = roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id
            }).ToList();
            return PartialView("_AddUser", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    Name = model.Name,
                    UserName = model.UserName,
                    Email = model.Email
                };
                IdentityResult result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    ApplicationRole applicationRole = await roleManager.FindByIdAsync(model.ApplicationRoleId);
                    if (applicationRole != null)
                    {
                        IdentityResult roleResult = await userManager.AddToRoleAsync(user, applicationRole.Name);
                        if (roleResult.Succeeded)
                        {
                            return RedirectToAction("Index");
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AddEmployee()
        {
            return RedirectToAction("NewEmployee");
        }

        [HttpGet]
        public IActionResult NewEmployee()
        {
            Employees model = new Employees();
            using (var context = new DataModel())
            {
                model.TradingEntities = context.TradingEntity.Select(r => new SelectListItem
                {
                    Text = r.Description,
                    Value = r.Id.ToString()
                }).ToList();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> NewEmployee(Employees model)
        {
            using (var context = new DataModel())
            {
                model.TradingEntities = context.TradingEntity.Select(r => new SelectListItem
                {
                    Text = r.Description,
                    Value = r.Id.ToString()
                }).ToList();
            }

            if (model.FirstName == null || model.FirstName == string.Empty)
            {
                ModelState.AddModelError(string.Empty, "Please Enter a First Name");
                return View(model); 
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction("Index");

                //ApplicationUser user = new ApplicationUser
                //{
                //    Name = model.Name,
                //    UserName = model.UserName,
                //    Email = model.Email
                //};
                //IdentityResult result = await userManager.CreateAsync(user, model.Password);
                //if (result.Succeeded)
                //{
                //    ApplicationRole applicationRole = await roleManager.FindByIdAsync(model.ApplicationRoleId);
                //    if (applicationRole != null)
                //    {
                //        IdentityResult roleResult = await userManager.AddToRoleAsync(user, applicationRole.Name);
                //        if (roleResult.Succeeded)
                //        {
                //            return RedirectToAction("Index");
                //        }
                //    }
                //}
                //else
                //{
                //    return RedirectToAction("Index");
                //}
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            EditUserViewModel model = new EditUserViewModel();
            model.ApplicationRoles = roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id
            }).ToList();

            if (!string.IsNullOrEmpty(id))
            {
                ApplicationUser user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    model.Name = user.Name;
                    model.Email = user.Email;
                    model.ApplicationRoleId = roleManager.Roles.Single(r => r.Name == userManager.GetRolesAsync(user).Result.Single()).Id;
                }
            }
            return PartialView("_EditUser", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(string id, EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    user.Name = model.Name;
                    user.Email = model.Email;
                    string existingRole = userManager.GetRolesAsync(user).Result.Single();
                    string existingRoleId = roleManager.Roles.Single(r => r.Name == existingRole).Id;
                    IdentityResult result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        if (existingRoleId != model.ApplicationRoleId)
                        {
                            IdentityResult roleResult = await userManager.RemoveFromRoleAsync(user, existingRole);
                            if (roleResult.Succeeded)
                            {
                                ApplicationRole applicationRole = await roleManager.FindByIdAsync(model.ApplicationRoleId);
                                if (applicationRole != null)
                                {
                                    IdentityResult newRoleResult = await userManager.AddToRoleAsync(user, applicationRole.Name);
                                    if (newRoleResult.Succeeded)
                                    {
                                        return RedirectToAction("Index");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(string id)
        {
            string name = string.Empty;
            if (!String.IsNullOrEmpty(id))
            {
                ApplicationUser applicationUser = await userManager.FindByIdAsync(id);
                if (applicationUser != null)
                {
                    name = applicationUser.Name;
                }
            }
            return PartialView("_DeleteUser", name);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id, EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    IdentityResult result = await userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Index");
        }
    }
}
