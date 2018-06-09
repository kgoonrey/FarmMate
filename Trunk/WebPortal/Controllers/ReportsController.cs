using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using WebPortal.Models;

namespace WebPortal.Controllers
{
    [Authorize]
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

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

                var data = from t in context.Timesheets
                           join e in context.Employees on t.Employee equals e.Id
                           join te in context.TradingEntity on t.TradingEntity equals te.Id
                           where t.StartDateTime >= reportParameters.StartDate.Date && t.EndDateTime < reportParameters.EndDate.Date.AddDays(1).AddSeconds(-1) && (reportParameters.Employee == -1 || e.Id == reportParameters.TradingEntity) && (reportParameters.TradingEntity == -1 || te.Id == reportParameters.TradingEntity)
                           orderby te.Description, e.FirstName, t.StartDateTime
                           select new { t.StartDateTime, t.EndDateTime, t.BreakAmount, EmployeeId = e.Id, Name = e.FirstName + ' ' + e.LastName, TradingEntityId = te.Id, TradingEntityDescription = te.Description, Notes = t.Notes };

                using (var w = new StreamWriter(filePath))
                {
                    w.WriteLine("Date,Start Time,End Time,Break Amount (Min),Hours Worked,Notes");
                    w.Flush();
                    w.WriteLine(",,,,,");
                    w.Flush();

                    var lastTradingEntity = -1;
                    var lastTradingEntityDescription = string.Empty;
                    var lastEmployee = -1;
                    var lastEmployeeName = string.Empty;
                    var employeeHoursWorkedTotal = 0m;
                    var tradingEntityWorkedTotal = 0m;
                    var reportWorkedTotal = 0m;

                    foreach (var row in data)
                    {
                        var hoursWorked = decimal.Parse((row.EndDateTime - row.StartDateTime).TotalHours.ToString("N2")) - (row.BreakAmount / 60);

                        if (lastTradingEntity != row.TradingEntityId)
                        {
                            if (lastEmployee != -1)
                            {
                                w.WriteLine($",,Total,,{employeeHoursWorkedTotal.ToString("N2")}");
                                w.Flush();

                                w.WriteLine(",,,,,");
                                w.Flush();

                                employeeHoursWorkedTotal = 0m;
                            }

                            if (lastTradingEntity != -1)
                            {
                                w.WriteLine($"{lastTradingEntityDescription} Total,,,,{tradingEntityWorkedTotal.ToString("N2")},");
                                w.Flush();

                                w.WriteLine(",,,,,");
                                w.Flush();

                                tradingEntityWorkedTotal = 0m;
                            }

                            lastTradingEntity = row.TradingEntityId;
                            lastTradingEntityDescription = row.TradingEntityDescription;
                            lastEmployee = -1;
                            w.WriteLine($"{lastTradingEntityDescription},,,,,");
                            w.Flush();
                        }

                        if (lastEmployee != row.EmployeeId)
                        {
                            if (lastEmployee != -1)
                            {
                                w.WriteLine($",,Total,,{employeeHoursWorkedTotal.ToString("N2")},");
                                w.Flush();

                                w.WriteLine(",,,,,");
                                w.Flush();

                                employeeHoursWorkedTotal = 0m;
                            }

                            lastEmployee = row.EmployeeId;
                            w.WriteLine($"{row.Name},,,,,");
                            w.Flush();
                        }

                        employeeHoursWorkedTotal += hoursWorked;
                        tradingEntityWorkedTotal += hoursWorked;
                        reportWorkedTotal += hoursWorked;

                        w.WriteLine($"{row.StartDateTime.ToShortDateString()},{row.StartDateTime.ToShortTimeString()},{row.EndDateTime.ToShortTimeString()},{row.BreakAmount.ToString("N2")},{hoursWorked.ToString("N2")},{row.Notes.Replace("\n", " ")}");
                        w.Flush();
                    }

                    if (lastEmployee != -1)
                    {
                        w.WriteLine($",,Total,,{employeeHoursWorkedTotal.ToString("N2")},");
                        w.Flush();

                        w.WriteLine(",,,,");
                        w.Flush();
                    }

                    if (lastTradingEntity != -1)
                    {
                        w.WriteLine($"{lastTradingEntityDescription} Total,,,,{tradingEntityWorkedTotal.ToString("N2")},");
                        w.Flush();

                        w.WriteLine(",,,,,");
                        w.Flush();
                    }

                    if (reportWorkedTotal != tradingEntityWorkedTotal)
                    {
                        w.WriteLine($"Report Total,,,,{reportWorkedTotal.ToString("N2")},");
                        w.Flush();
                    }
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