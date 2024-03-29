﻿using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPortal.Models;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Identity;
using WebPortal.Data;
using Microsoft.AspNetCore.Authorization;

namespace WebPortal.Controllers
{
    [Authorize]
    [Produces("application/json")]
    public class DataController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public DataController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet]
        [Route("api/Data/GetTradingEntity")]
        public JsonResult GetTradingEntity()
        {
            using (var context = new DataModel())
            {
                var tradingEntity = context.Employees.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.FirstName} {a.LastName}"
                }).ToListAsync();
                ViewBag.TradingEntitys = tradingEntity;
                return Json(tradingEntity);
            }
        }

        public class TradingEntity
        {
            public int Id { get; set; }
            public string Description { get; set; }
        }

        [HttpPost]
        [Route("api/Data/GetEmployee")]
        public JsonResult GetEmployee([FromBody]TradingEntity id)
        {
            var user = userManager.GetUserAsync(HttpContext.User);
            var list = new List<Employees>();
            if (id == null)
                return Json(list);

            using (var context = new DataModel())
            {
                var userEmployeeAccess = context.UserEmployeeAccess.Where(x => x.UserId == user.Result.Id).ToList();
                var employees = context.Employees.Where(x => x.Active && x.TradingEntity == id.Id && userEmployeeAccess.FirstOrDefault(y => y.EmployeeId == x.Id) != null).ToList();
                var roles = context.AspNetRoles.FirstOrDefault(x => x.Name == "Admin");
                var adminUserList = context.AspNetUserRoles.Where(x => x.RoleId == roles.Id).ToList();

                if (adminUserList.FirstOrDefault(x => x.UserId == user.Result.Id) != null)
                    employees = context.Employees.Where(x => x.Active && x.TradingEntity == id.Id).ToList();

                foreach (var employeeRow in employees)
                {
                    var employee = new Employees();
                    employee.Id = employeeRow.Id;
                    employee.FirstName = employeeRow.FirstName;
                    employee.LastName = employeeRow.LastName;

                    list.Add(employee);
                }
            }

            return Json(list);
        }

        [HttpPost]
        [Route("api/Data/GetPreviousTimesheet")]
        public JsonResult GetPreviousTimesheet([FromBody]Timesheets timesheet)
        {
            using (var context = new DataModel())
            {
                var timesheetRow = context.Timesheets.FirstOrDefault(x => x.Employee == timesheet.Employee && x.TradingEntity == timesheet.TradingEntity && x.StartDateTime >= timesheet.StartDateTime.Date && x.EndDateTime < timesheet.EndDateTime.Date.AddDays(1).AddSeconds(-1) && x.Type == timesheet.Type);
                if (timesheetRow == null)
                    return Json(null);

                timesheet.Id = timesheetRow.Id;
                timesheet.StartDateTime = timesheetRow.StartDateTime;
                timesheet.EndDateTime = timesheetRow.EndDateTime;
                timesheet.BreakAmount = timesheetRow.BreakAmount;
                timesheet.Notes = timesheetRow.Notes;
            }

            return Json(timesheet);
        }

        [HttpPost]
        [Route("api/Data/GetMonthlyTimesheets")]
        public JsonResult GetMonthlyTimesheets([FromBody]Timesheets timesheet)
        {
            using (var context = new DataModel())
            {
                var timesheetRows = context.Timesheets.Where(x => x.Employee == timesheet.Employee && x.TradingEntity == timesheet.TradingEntity && x.StartDateTime >= System.DateTime.Today.Date.AddYears(-1) && x.EndDateTime <= System.DateTime.Today.Date.AddYears(1).AddDays(1).AddSeconds(-1)).ToList();
                return Json(timesheetRows);
            }
        }

        [HttpPost]
        [Route("api/Data/GetEmployeeDefaults")]
        public JsonResult GetEmployeeDefaults([FromBody]Timesheets timesheet)
        {
            if (timesheet == null)
                return Json(timesheet);

            using (var context = new DataModel())
            {
                var employee = context.Employees.FirstOrDefault(x => x.Id == timesheet.Employee && x.TradingEntity == timesheet.TradingEntity);
                if (employee == null)
                    return Json(null);

                timesheet.StartDateTime = System.DateTime.Today + employee.DefaultStartTime.TimeOfDay;
                timesheet.EndDateTime = System.DateTime.Today + employee.DefaultEndTime.TimeOfDay;
                timesheet.BreakAmount = employee.DefaultBreakAmount;

                return Json(timesheet);
            }
        }

        [HttpGet]
        [Route("api/Data/GetNozzleConfigTankVolume/{id}")]
        public int GetNozzleConfigTankVolume(int id)
        {
            using (var context = new DataModel())
            {
                var config = context.SprayNozzleConfiguration.FirstOrDefault(x => x.Id == id);
                return config?.TankVolume ?? 0;
            }
        }

        [HttpGet]
        [Route("api/Data/GetProductsByType/{productType}")]
        public List<Products> GetProductsByType(string productType)
        {
            using (var context = new DataModel())
            {
                return context.Products.Where(x => productType == "All" || x.Type == productType).ToList();
            }
        }

        [HttpGet]
        [Route("api/Data/GetProductsByGroup/{productGroup}")]
        public List<Products> GetProductsByGroup(string productGroup)
        {
            using (var context = new DataModel())
            {
                int.TryParse(productGroup, out var id);
                return context.Products.Where(x => productGroup == "All" || x.ProductGroup == id).ToList();
            }
        }
    }
}
