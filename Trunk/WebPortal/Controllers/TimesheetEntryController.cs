using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPortal.Models;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Identity;
using WebPortal.Data;
using Microsoft.AspNetCore.Authorization;

namespace WebPortal.Controllers
{
    [Authorize]
    public class TimesheetEntryController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public TimesheetEntryController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            using (var context = new DataModel())
            {
                var user = await userManager.GetUserAsync(HttpContext.User);
                var userEmployeeAccess = await context.UserEmployeeAccess.Where(x => x.UserId == user.Id).ToListAsync();
                var employees = await context.Employees.Where(x => userEmployeeAccess.FirstOrDefault(y => y.EmployeeId == x.Id) != null).ToListAsync();

                var roles = await context.AspNetRoles.FirstOrDefaultAsync(x => x.Name == "Admin");
                var adminUserList = await context.AspNetUserRoles.Where(x => x.RoleId == roles.Id).ToListAsync();

                if(adminUserList.FirstOrDefault(x=> x.UserId == user.Id) != null)
                    employees = await context.Employees.ToListAsync();

                var tradingEntity = await context.TradingEntity.Where(x=> employees.FirstOrDefault(y => y.TradingEntity == x.Id) != null).Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Description
                }).ToListAsync();
                ViewBag.TradingEntitys = tradingEntity;

                var employeesList = employees.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.FirstName} {a.LastName}"
                }).ToList();
                ViewBag.Employees = employeesList;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            using (var context = new DataModel())
            {
                var tradingEntity = await context.TradingEntity.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Description
                }).ToListAsync();
                ViewBag.TradingEntitys = tradingEntity;
            }
            return View();
        }

        [HttpPost]
        [Route("api/TimesheetEntry/SubmitTimesheet")]
        public string SubmitTimesheet([FromBody]Timesheets timesheet)
        {
            try
            {
                var updateExisting = (timesheet.Id != Guid.Empty);
                if (!updateExisting)
                    timesheet.Id = Guid.NewGuid();

                using (var context = new DataModel())
                {
                    if (updateExisting)
                        context.Update(timesheet);
                    else
                        context.Add(timesheet);

                    context.SaveChanges();
                }

                return "Timesheet Submitted Successfully";
            }
            catch { }

            return "Timesheet failed, Please try again.";
        }

    }
}