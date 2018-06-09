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

namespace WebPortal.Controllers
{
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
                var tradingEntity = await context.TradingEntity.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Description
                }).ToListAsync();
                ViewBag.TradingEntitys = tradingEntity;

                var employees = await context.Employees.Where(x=> x.Id == 200).Select(a => new SelectListItem //return blank
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.FirstName} {a.LastName}"
                }).ToListAsync();
                ViewBag.Employees = employees;
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