using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPortal.Models;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Identity;
using WebPortal.Data;

namespace WebPortal.Controllers
{
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
            var list = new List<Employees>();
            using (var context = new DataModel())
            {
                var employees = context.Employees.Where(x => x.TradingEntity == id.Id);
                foreach(var employeeRow in employees)
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
                var timesheetRow = context.Timesheets.FirstOrDefault(x => x.Employee == timesheet.Employee && x.TradingEntity == timesheet.TradingEntity && x.StartDateTime >= timesheet.StartDateTime.Date && x.EndDateTime < timesheet.EndDateTime.Date.AddDays(1).AddSeconds(-1));
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
    }
}
