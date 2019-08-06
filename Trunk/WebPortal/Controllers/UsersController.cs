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

            List<Employees> model2 = new List<Employees>();
            using (var context = new DataModel())
            {
                model2 = context.Employees.Where(x=> x.Active).ToList();

                foreach (var item in model2)
                    item.TradingEntityDescription = item.TradingEntity + " - " + context.TradingEntity.FirstOrDefault(x => x.Id == item.TradingEntity).Description;
            }

            var newTest = Tuple.Create(model, model2);
            return View(newTest);
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
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(UserViewModel model)
        {
            if(model.UserName == null || model.UserName == string.Empty)
                ModelState.AddModelError(string.Empty, "Username missing");

            if (model.Name == null || model.Name == string.Empty)
                ModelState.AddModelError(string.Empty, "Name missing");

            if (model.Email == null || model.Email == string.Empty || !model.Email.Contains("@"))
                ModelState.AddModelError(string.Empty, "Invalid Email");

            if (model.ApplicationRoleId == null || model.ApplicationRoleId == "Please select")
                ModelState.AddModelError(string.Empty, "Please select a valid role");

            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    Name = model.Name,
                    UserName = model.UserName,
                    Email = model.Email
                };

                ApplicationRole applicationRole = await roleManager.FindByIdAsync(model.ApplicationRoleId);
                if (applicationRole == null)
                {
                    ModelState.AddModelError(string.Empty, "Please select a valid role");
                    return View(model);
                }

                IdentityResult result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    IdentityResult roleResult = await userManager.AddToRoleAsync(user, applicationRole.Name);
                    if (roleResult.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.ApplicationRoles = roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id
            }).ToList();

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
                            ApplicationRole applicationRole = await roleManager.FindByIdAsync(model.ApplicationRoleId);
                            if (applicationRole != null)
                            {
                                IdentityResult roleResult = await userManager.RemoveFromRoleAsync(user, existingRole);
                                if (roleResult.Succeeded)
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
            if (!string.IsNullOrEmpty(id))
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
                        using (var context = new DataModel())
                        {
                            var links = context.UserEmployeeAccess.Where(x => x.UserId == id);
                            foreach (var link in links.ToList())
                                context.UserEmployeeAccess.Remove(link);

                            context.SaveChanges();
                        }

                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> SendAccountDetails(string id)
        {
            string email = string.Empty;
            if (!string.IsNullOrEmpty(id))
            {
                ApplicationUser applicationUser = await userManager.FindByIdAsync(id);
                if (applicationUser != null)
                {
                    email = applicationUser.Email;
                }
            }
            return PartialView("_SendAccountDetails", email);
        }

        [HttpPost]
        public IActionResult SendAccountDetails(string id, EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = userManager.FindByIdAsync(id).Result;

                if (user == null || user.Email == null || user.Email == string.Empty)
                {
                    return RedirectToAction("Index");
                }

                var token = userManager.GeneratePasswordResetTokenAsync(user).Result;

                var resetLink = Url.Action("ResetPassword",
                                "Account", new { token = token },
                                 protocol: HttpContext.Request.Scheme);

                using (var context = new DataModel())
                {
                    var tradingEntity = context.TradingEntity.FirstOrDefault();
                    Tools.SendEmail(tradingEntity, user.Email, "Farm Mate Account", $"Hi {user.Name},{Environment.NewLine}{Environment.NewLine}Your account has been created for Farm Mate, your username is {user.UserName}{Environment.NewLine}{Environment.NewLine}Please click the link below to set password and starting using.{Environment.NewLine}{Environment.NewLine}{resetLink}");
                }

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
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
        public IActionResult NewEmployee(Employees model)
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

            if (model.LastName == null)
                model.LastName = string.Empty;

            if (model.Occupation == null)
                model.Occupation = string.Empty;

            model.Active = true;

            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    context.Add(model);
                    context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult EditEmployee(int id)
        {
            Employees model = null;
            using (var context = new DataModel())
            {
                model = context.Employees.FirstOrDefault(x => x.Id == id);
                model.TradingEntities = context.TradingEntity.Select(r => new SelectListItem
                {
                    Text = r.Description,
                    Value = r.Id.ToString()
                }).ToList();
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult EditEmployee(int id, Employees model)
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

            if (model.LastName == null)
                model.LastName = string.Empty;

            if (model.Occupation == null)
                model.Occupation = string.Empty;

            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    context.Update(model);
                    context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteEmployee(int id)
        {
            string name = string.Empty;
            using (var context = new DataModel())
            {
                var employee = context.Employees.FirstOrDefault(x => x.Id == id);
                if (employee != null)
                    name = (employee.FirstName + " " + employee.LastName).Trim();
            }

            return PartialView("_DeleteEmployee", name);
        }

        [HttpPost]
        public IActionResult DeleteEmployee(int id, Employees model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    model = context.Employees.FirstOrDefault(x => x.Id == id);
                    var access = context.UserEmployeeAccess.Where(x => x.EmployeeId == id);
                    foreach (var item in access.ToList())
                        context.UserEmployeeAccess.Remove(item);

                    if (context.Timesheets.Any(x => x.Employee == id))
                    {
                        model.Active = false;
                        context.Update(model);
                    }
                    else
                    {
                        context.Employees.Remove(model);
                    }
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult LinkEmployee(string id)
        {
            LinkEmployeeViewModel model = new LinkEmployeeViewModel();
            using (var context = new DataModel())
            {
                model.SelectedList = new List<Employees>();
                model.OptionList = context.Employees.ToList();

                foreach(var item in context.UserEmployeeAccess.Where(x => x.UserId == id).ToList())
                {
                    var employee = context.Employees.FirstOrDefault(x => x.Id == item.EmployeeId);
                    var optionEmployee = model.OptionList.FirstOrDefault(x => x.Id == item.EmployeeId);
                    if (optionEmployee != null)
                        model.OptionList.Remove(optionEmployee);

                    model.SelectedList.Add(employee);
                }
            }

            foreach (var row in model.SelectedList)
                row.Name = (row.FirstName + " " + row.LastName).Trim() + " - " + row.TradingEntity;

            foreach (var row in model.OptionList)
                row.Name = (row.FirstName + " " + row.LastName).Trim() + " - " + row.TradingEntity;

            model.UserId = id;
            return PartialView("_LinkEmployee", model);
        }

        [HttpPost]
        [Route("api/Users/SaveLinkedEmployee")]
        public void SaveLinkedEmployee([FromBody]SaveEmployeeViewModel[] model)
        {
            if (ModelState.IsValid)
            {
                var id = model.FirstOrDefault()?.UserId;
                if (id == null)
                    return;

                using (var context = new DataModel())
                {
                    var currentLink = context.UserEmployeeAccess.Where(x => x.UserId == id);
                    foreach (var row in currentLink.ToList())
                        context.UserEmployeeAccess.Remove(row);

                    foreach (var row in model)
                    {
                        if (row.EmployeeId == -1)
                            continue;

                        var link = new UserEmployeeAccess();
                        link.UserId = id;
                        link.EmployeeId = row.EmployeeId;
                        context.UserEmployeeAccess.Add(link);
                    }

                    context.SaveChanges();
                }
            }
        }
    }
}
