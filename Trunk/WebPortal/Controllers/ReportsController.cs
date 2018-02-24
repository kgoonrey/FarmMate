using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using WebPortal.Models;

namespace WebPortal.Controllers
{
    public class ReportsController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private static Guid _reportId;
        private static string _reportName;

        public class ReportParameters
        {
            public DateTime StartDate { get; set; } = DateTime.Now;
            public DateTime EndDate { get; set; } = DateTime.Now;
            public int Employee { get; set; } = -1;
            public int TradingEntity { get; set; } = -1;
        }

        public ReportsController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            using (var context = new DataModel())
            {
                var tradingEntity = await context.TradingEntity.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Description
                }).ToListAsync();
                ViewBag.TradingEntitys = tradingEntity;

                var employees = await context.Employees.Where(x => x.Id == 200).Select(a => new SelectListItem //return blank
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.FirstName} {a.LastName}"
                }).ToListAsync();
                ViewBag.Employees = employees;
            }

            return View();
        }

        [HttpPost]
        [Route("api/Reports/GenerateTimesheetReport")]
        public JsonResult GenerateTimesheetReport([FromBody]ReportParameters reportParameters)
        {
            _reportId = Guid.NewGuid();
            var filePath = _hostingEnvironment.WebRootPath + "\\Reports\\" + _reportId;
            _reportName = $"Timesheets_{reportParameters.StartDate.ToString("yyyyMMdd")}-{reportParameters.EndDate.ToString("yyyyMMdd")}.csv";
            using (var context = new DataModel())
            {
                var tradingEntityRow = context.TradingEntity.FirstOrDefault(x => x.Id == reportParameters.TradingEntity);
                var employeeRow = context.Employees.FirstOrDefault(x => x.Id == reportParameters.Employee);

                if (tradingEntityRow != null)
                    _reportName = tradingEntityRow.Description + "_" + _reportName;

                if (employeeRow != null)
                    _reportName = $"{employeeRow.FirstName}_{employeeRow.LastName}_{_reportName}";
            }

            var query = @"SELECT t.StartDateTime, t.EndDateTime, t.BreakAmount, e.Id AS EmployeeId, e.FirstName + ' ' + e.LastName AS Name, te.Id AS TradingEntityId, te.Description AS TradingEntityDescription  
                        FROM Timesheets t
                        INNER JOIN Employees e on t.Employee = e.Id
                        INNER JOIN TradingEntity te on te.Id = t.TradingEntity
                        WHERE t.StartDateTime BETWEEN @StartDate AND @EndDate ";

            if (reportParameters.Employee != -1)
                query += " AND e.Id = @Employee ";

            if (reportParameters.TradingEntity != -1)
                query += " AND te.Id = @TradingEntity ";

            query += "  ORDER BY te.Description, e.FirstName, t.StartDateTime";

            var results = SqlHelper.RunQuery(query, new Dictionary<string, object>() { { "@StartDate", reportParameters.StartDate.Date }, { "@EndDate", reportParameters.EndDate.AddDays(1).AddSeconds(-1) }, { "@Employee", reportParameters.Employee }, { "@TradingEntity", reportParameters.TradingEntity } });

            using (var w = new StreamWriter(filePath))
            {
                w.WriteLine("Date,Start Time,End Time,Break Amount (Min),Hours Worked");
                w.Flush();
                w.WriteLine(",,,,");
                w.Flush();

                var lastTradingEntity = -1;
                var lastTradingEntityDescription = string.Empty;
                var lastEmployee = -1;
                var lastEmployeeName = string.Empty;
                var employeeBreakTotal = 0m;
                var employeeHoursWorkedTotal = 0m;
                var tradingEntityBreakTotal = 0m;
                var tradingEntityWorkedTotal = 0m;
                var reportBreakTotal = 0m;
                var reportWorkedTotal = 0m;

                foreach (DataRow row in results.Rows)
                {
                    var startDate = (DateTime)row["StartDateTime"];
                    var endDate = (DateTime)row["EndDateTime"];
                    var breakAmount = (decimal)row["BreakAmount"];
                    var employeeId = (int)row["EmployeeId"];
                    var name = (string)row["Name"];
                    var tradingEntityId = (int)row["TradingEntityId"];
                    var tradingEntityDescription = (string)row["TradingEntityDescription"];
                    var hoursWorked = decimal.Parse((endDate - startDate).TotalHours.ToString("N2")) - (breakAmount / 60);

                    if (lastTradingEntity != tradingEntityId)
                    {
                        if (lastEmployee != -1)
                        {
                            w.WriteLine($",,Total,{employeeBreakTotal.ToString("N2")},{employeeHoursWorkedTotal.ToString("N2")}");
                            w.Flush();

                            w.WriteLine(",,,,");
                            w.Flush();

                            employeeBreakTotal = employeeHoursWorkedTotal = 0m;
                        }

                        if (lastTradingEntity != -1)
                        {
                            w.WriteLine($"{lastTradingEntityDescription} Total,,,{tradingEntityBreakTotal.ToString("N2")},{tradingEntityWorkedTotal.ToString("N2")}");
                            w.Flush();

                            w.WriteLine(",,,,");
                            w.Flush();

                            tradingEntityBreakTotal = tradingEntityWorkedTotal = 0m;
                        }

                        lastTradingEntity = tradingEntityId;
                        lastTradingEntityDescription = tradingEntityDescription;
                        lastEmployee = -1;
                        w.WriteLine($"{lastTradingEntityDescription},,,,");
                        w.Flush();
                    }

                    if (lastEmployee != employeeId)
                    {
                        if (lastEmployee != -1)
                        {
                            w.WriteLine($",,Total,{employeeBreakTotal.ToString("N2")},{employeeHoursWorkedTotal.ToString("N2")}");
                            w.Flush();

                            w.WriteLine(",,,,");
                            w.Flush();

                            employeeBreakTotal = employeeHoursWorkedTotal = 0m;
                        }

                        lastEmployee = employeeId;
                        w.WriteLine($"{name},,,,");
                        w.Flush();
                    }

                    employeeBreakTotal += breakAmount;
                    tradingEntityBreakTotal += breakAmount;
                    reportBreakTotal += breakAmount;
                    employeeHoursWorkedTotal += hoursWorked;
                    tradingEntityWorkedTotal += hoursWorked;
                    reportWorkedTotal += hoursWorked;

                    w.WriteLine($"{startDate.ToShortDateString()},{startDate.ToShortTimeString()},{endDate.ToShortTimeString()},{breakAmount.ToString("N2")},{hoursWorked.ToString("N2")}");
                    w.Flush();
                }

                if (lastEmployee != -1)
                {
                    w.WriteLine($",,Total,{employeeBreakTotal.ToString("N2")},{employeeHoursWorkedTotal.ToString("N2")}");
                    w.Flush();

                    w.WriteLine(",,,,");
                    w.Flush();
                }

                if (lastTradingEntity != -1)
                {
                    w.WriteLine($"{lastTradingEntityDescription} Total,,,{tradingEntityBreakTotal.ToString("N2")},{tradingEntityWorkedTotal.ToString("N2")}");
                    w.Flush();

                    w.WriteLine(",,,,");
                    w.Flush();
                }

                if(reportWorkedTotal != tradingEntityWorkedTotal)
                {
                    w.WriteLine($"Report Total,,,{reportBreakTotal.ToString("N2")},{reportWorkedTotal.ToString("N2")}");
                    w.Flush();
                }
            }

            return Json("Success");
        }

        public FileResult Download()
        {
            var fileName = _reportName;
            var filePath = _hostingEnvironment.WebRootPath + "\\Reports\\" + _reportId;
            var fileExists = System.IO.File.Exists(filePath);
            var fs = System.IO.File.OpenRead(filePath);
            return File(fs, "application/csv", fileName);
        }
    }
}