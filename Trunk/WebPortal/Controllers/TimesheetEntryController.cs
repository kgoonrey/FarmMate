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
        public enum TimeSheetTypeEnum
        {
            Work,
            Sick,
            AnnualLeave
        }

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
                var employees = await context.Employees.Where(x => x.Active && userEmployeeAccess.FirstOrDefault(y => y.EmployeeId == x.Id) != null).ToListAsync();

                var roles = await context.AspNetRoles.FirstOrDefaultAsync(x => x.Name == "Admin");
                var adminUserList = await context.AspNetUserRoles.Where(x => x.RoleId == roles.Id).ToListAsync();

                if(adminUserList.FirstOrDefault(x=> x.UserId == user.Id) != null)
                    employees = await context.Employees.Where(x=> x.Active).ToListAsync();

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
        public async Task<string> SubmitTimesheet([FromBody]Timesheets timesheet)
        {
            try
            {
                using (var context = new DataModel())
                {
                    var user = await userManager.GetUserAsync(HttpContext.User);
                    var existingTimesheet = context.Timesheets.FirstOrDefault(x => x.Employee == timesheet.Employee && x.TradingEntity == timesheet.TradingEntity && x.StartDateTime.Date == timesheet.StartDateTime.Date && x.Type == timesheet.Type);
                    var exists = true;

                    if(existingTimesheet == null)
                    {
                        existingTimesheet = new Timesheets();
                        existingTimesheet.Id = Guid.NewGuid();
                        existingTimesheet.Type = timesheet.Type;
                        exists = false;
                    }

                    existingTimesheet.Employee = timesheet.Employee;
                    existingTimesheet.TradingEntity = timesheet.TradingEntity;
                    existingTimesheet.StartDateTime = timesheet.StartDateTime;
                    existingTimesheet.EndDateTime = timesheet.EndDateTime;
                    existingTimesheet.BreakAmount = 0;
                    existingTimesheet.Notes = timesheet.Notes;
                    existingTimesheet.AuditDateTime = DateTime.Now;
                    existingTimesheet.AuditUser = user.Id;

                    if (exists)
                        context.Update(existingTimesheet);
                    else
                        context.Add(existingTimesheet);

                    context.SaveChanges();
                }

                return "Timesheet Submitted Successfully";
            }
            catch { }

            return "Timesheet failed, Please try again.";
        }

        [HttpPost]
        [Route("api/TimesheetEntry/DeleteTimesheet")]
        public async Task<string> DeleteTimesheet([FromBody]Timesheets timesheet)
        {
            try
            {
                using (var context = new DataModel())
                {
                    var user = await userManager.GetUserAsync(HttpContext.User);
                    var existingTimesheets = context.Timesheets.Where(x => x.Employee == timesheet.Employee && x.TradingEntity == timesheet.TradingEntity && x.StartDateTime.Date == timesheet.StartDateTime.Date);

                    if (!existingTimesheets.Any())
                    {
                        return "Timesheet Doesn't Exist";
                    }

                    foreach (var row in existingTimesheets)
                        context.Remove(row);

                    context.SaveChanges();
                }

                return "Timesheet Deleted Successfully";
            }
            catch { }

            return "Timesheet failed to delete, Please try again.";
        }
    }
}