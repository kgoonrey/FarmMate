using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPortal.Models;
using System.Collections.Generic;
using System.Data;

namespace WebPortal.Controllers
{
    [Produces("application/json")]
    public class DataController : Controller
    {
        [HttpGet]
        [Route("api/Data/GetTradingEntity")]
        public JsonResult GetTradingEntity()
        {
            //var dataTable = SqlHelper.RunQuery("select * from TradingEntity", new Dictionary<string, object>());
            //var list = new List<string>();

            //foreach (DataRow row in dataTable.Rows)
            //{
            //    var id = (int)row["Id"];
            //    var description = (string)row["Description"];
            //    list.Add(description);
            //}

            //return Json(list);

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

            // return Json(list); //TODO: Work out how to do it with the id
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
            var query = @"SELECT E.Id, E.FirstName, E.LastName 
                        FROM Employees E
                        INNER JOIN TradingEntity TE on E.TradingEntity = TE.Id
                        WHERE E.TradingEntity = @Id";

            var dataTable = SqlHelper.RunQuery(query, new Dictionary<string, object>() { { "@Id", id.Id } });
            var list = new List<Employees>();

            foreach (DataRow row in dataTable.Rows)
            {
                var idValue = (int)row["Id"];
                var firstName = (string)row["FirstName"];
                var lastName = (string)row["LastName"];

                var employee = new Employees();
                employee.Id = idValue;
                employee.FirstName = firstName;
                employee.LastName = lastName;

                list.Add(employee);
            }

            return Json(list);
        }

        [HttpPost]
        [Route("api/Data/GetPreviousTimesheet")]
        public JsonResult GetPreviousTimesheet([FromBody]Timesheets timesheet)
        {
            var query = @"SELECT TOP 1 * FROM Timesheets
                          WHERE Employee = @Employee AND TradingEntity = @TradingEntity AND StartDateTime BETWEEN @StartDate AND @EndDate";

            var dataTable = SqlHelper.RunQuery(query, new Dictionary<string, object>() { { "@Employee", timesheet.Employee }, { "@TradingEntity", timesheet.TradingEntity }, { "@StartDate", timesheet.StartDateTime.Date }, { "@EndDate", timesheet.StartDateTime.Date.AddDays(1).AddSeconds(-1) } });
            if (dataTable.Rows.Count == 0)
                return Json(null);

            timesheet.Id = (System.Guid)dataTable.Rows[0]["Id"];
            timesheet.StartDateTime = (System.DateTime)dataTable.Rows[0]["StartDateTime"];
            timesheet.EndDateTime = (System.DateTime)dataTable.Rows[0]["EndDateTime"];
            timesheet.BreakAmount = (decimal)dataTable.Rows[0]["BreakAmount"];
            return Json(timesheet);
        }
    }
}
