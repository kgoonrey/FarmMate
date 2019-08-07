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
                           where t.StartDateTime >= reportParameters.StartDate.Date && t.EndDateTime < reportParameters.EndDate.Date.AddDays(1).AddSeconds(-1) && (reportParameters.Employee == -1 || e.Id == reportParameters.Employee) && (reportParameters.TradingEntity == -1 || te.Id == reportParameters.TradingEntity)
                           orderby te.Description, e.FirstName, t.StartDateTime
                           select new { t.Type, t.StartDateTime, t.EndDateTime, t.BreakAmount, EmployeeId = e.Id, Name = e.FirstName + ' ' + e.LastName, TradingEntityId = te.Id, TradingEntityDescription = te.Description, Notes = t.Notes };

                using (var w = new StreamWriter(filePath))
                {
                    w.WriteLine("Date,Start Time,End Time,Break Amount (Min),Total Hours,Type,Notes");
                    w.Flush();
                    w.WriteLine(",,,,,,");
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

                                w.WriteLine(",,,,,,");
                                w.Flush();

                                employeeHoursWorkedTotal = 0m;
                            }

                            if (lastTradingEntity != -1)
                            {
                                w.WriteLine($"{lastTradingEntityDescription} Total,,,,{tradingEntityWorkedTotal.ToString("N2")},");
                                w.Flush();

                                w.WriteLine(",,,,,,");
                                w.Flush();

                                tradingEntityWorkedTotal = 0m;
                            }

                            lastTradingEntity = row.TradingEntityId;
                            lastTradingEntityDescription = row.TradingEntityDescription;
                            lastEmployee = -1;
                            w.WriteLine($"{lastTradingEntityDescription},,,,,,");
                            w.Flush();
                        }

                        if (lastEmployee != row.EmployeeId)
                        {
                            if (lastEmployee != -1)
                            {
                                w.WriteLine($",,Total,,{employeeHoursWorkedTotal.ToString("N2")},");
                                w.Flush();

                                w.WriteLine(",,,,,,");
                                w.Flush();

                                employeeHoursWorkedTotal = 0m;
                            }

                            lastEmployee = row.EmployeeId;
                            w.WriteLine($"{row.Name},,,,,,");
                            w.Flush();
                        }

                        employeeHoursWorkedTotal += hoursWorked;
                        tradingEntityWorkedTotal += hoursWorked;
                        reportWorkedTotal += hoursWorked;

                        if (hoursWorked == 0)
                            continue;

                        w.WriteLine($"{row.StartDateTime.ToShortDateString()},{row.StartDateTime.ToShortTimeString()},{row.EndDateTime.ToShortTimeString()},{row.BreakAmount.ToString("N2")},{hoursWorked.ToString("N2")},{((TimesheetEntryController.TimeSheetTypeEnum)row.Type).ToString()}{row.Notes.Replace("\n", " ")}");
                        w.Flush();
                    }

                    if (lastEmployee != -1)
                    {
                        w.WriteLine($",,Total,,{employeeHoursWorkedTotal.ToString("N2")},");
                        w.Flush();

                        w.WriteLine(",,,,,");
                        w.Flush();
                    }

                    if (lastTradingEntity != -1)
                    {
                        w.WriteLine($"{lastTradingEntityDescription} Total,,,,{tradingEntityWorkedTotal.ToString("N2")},");
                        w.Flush();

                        w.WriteLine(",,,,,,");
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

        private class CanegrowersTimesheetPerDay
        {
            public DateTime Date { get; set; }
            public int EmployeeId { get; set; } = -1;
            public decimal OrdTime { get; set; } = 0m;
            public decimal WeekendTime { get; set; } = 0m;
            public decimal AnnualLeave { get; set; } = 0m;
            public decimal SickLeave { get; set; } = 0m;
            public decimal PublicHol { get; set; } = 0m;
            public decimal WorkedPublicHol { get; set; } = 0m;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("api/Reports/GenerateCanegrowersTimesheetReport")]
        public JsonResult GenerateCanegrowersTimesheetReport([FromBody]ReportParameters reportParameters)
        {
            _reportId = Guid.NewGuid();
            var filePath = _hostingEnvironment.WebRootPath + "\\Reports\\" + _reportId;
            _reportName = $"Canegrowers_Timesheets_{reportParameters.StartDate.ToString("yyyyMMdd")}-{reportParameters.EndDate.ToString("yyyyMMdd")}.csv";
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
                           where t.StartDateTime >= reportParameters.StartDate.Date && t.EndDateTime < reportParameters.EndDate.Date.AddDays(1).AddSeconds(-1) && (reportParameters.Employee == -1 || e.Id == reportParameters.Employee) && (reportParameters.TradingEntity == -1 || te.Id == reportParameters.TradingEntity)
                           orderby te.Description, e.FirstName, t.StartDateTime, t.Type
                           select new { t.Type, t.StartDateTime, t.EndDateTime, t.BreakAmount, EmployeeId = e.Id, Name = e.FirstName + ' ' + e.LastName, TradingEntityId = te.Id, TradingEntityDescription = te.Description, Notes = t.Notes };

                var publicHolidays = context.PublicHolidays.Where(x => x.Region == "QLD");

                var employeeList = new List<int>();
                var employeeNameLookup = new Dictionary<int, string>();

                foreach (var row in data)
                {
                    if (!employeeList.Contains(row.EmployeeId))
                    {
                        employeeList.Add(row.EmployeeId);
                        employeeNameLookup[row.EmployeeId] = row.Name;
                    }
                }

                var employeeTimesheets = new List<CanegrowersTimesheetPerDay>();

                using (var w = new StreamWriter(filePath))
                {

                    for (DateTime counter = reportParameters.StartDate; counter <= reportParameters.EndDate.Date.AddDays(1).AddSeconds(-1); counter = counter.AddDays(1))
                    {
                        var publicHoliday = publicHolidays.Any(x=> x.Date == counter);
                        if (publicHoliday)
                        {
                            foreach (var employee in employeeList)
                            {
                                var timesheet = GetEmployeeTimesheet(employeeTimesheets, counter, employee);

                                var worked = data.FirstOrDefault(x => x.EmployeeId == employee && x.StartDateTime.Date == counter.Date && x.Type == (int)TimesheetEntryController.TimeSheetTypeEnum.Work);
                                if (worked != null)
                                {
                                    var hoursWorked = decimal.Parse((worked.EndDateTime - worked.StartDateTime).TotalHours.ToString("N2")) - (worked.BreakAmount / 60);
                                    timesheet.WorkedPublicHol = hoursWorked;
                                }
                                else
                                {
                                    timesheet.PublicHol = 7.6m;
                                }
                            }
                        }
                        else if (counter.DayOfWeek == DayOfWeek.Saturday || counter.DayOfWeek == DayOfWeek.Sunday)
                        {
                            foreach (var employee in employeeList)
                            {
                                var timesheet = GetEmployeeTimesheet(employeeTimesheets, counter, employee);

                                var worked = data.FirstOrDefault(x => x.EmployeeId == employee && x.StartDateTime.Date == counter.Date && x.Type == (int)TimesheetEntryController.TimeSheetTypeEnum.Work);
                                if (worked != null)
                                {
                                    var hoursWorked = decimal.Parse((worked.EndDateTime - worked.StartDateTime).TotalHours.ToString("N2")) - (worked.BreakAmount / 60);
                                    timesheet.WeekendTime = hoursWorked;
                                }
                            }
                        }
                        else
                        {
                            foreach (var employee in employeeList)
                            {
                                var timesheet = GetEmployeeTimesheet(employeeTimesheets, counter, employee);

                                var worked = data.FirstOrDefault(x => x.EmployeeId == employee && x.StartDateTime.Date == counter.Date && x.Type == (int)TimesheetEntryController.TimeSheetTypeEnum.Work);
                                var sick = data.FirstOrDefault(x => x.EmployeeId == employee && x.StartDateTime.Date == counter.Date && x.Type == (int)TimesheetEntryController.TimeSheetTypeEnum.Sick);
                                var leave = data.FirstOrDefault(x => x.EmployeeId == employee && x.StartDateTime.Date == counter.Date && x.Type == (int)TimesheetEntryController.TimeSheetTypeEnum.AnnualLeave);

                                if (worked != null)
                                {
                                    var hoursWorked = decimal.Parse((worked.EndDateTime - worked.StartDateTime).TotalHours.ToString("N2")) - (worked.BreakAmount / 60);
                                    timesheet.OrdTime = hoursWorked;
                                }

                                if (sick != null)
                                {
                                    var hoursSick = decimal.Parse((sick.EndDateTime - sick.StartDateTime).TotalHours.ToString("N2"));
                                    if (hoursSick != 0)
                                    {
                                        var maxHoursSick = 7.6m;
                                        maxHoursSick -= timesheet.OrdTime;

                                        if (hoursSick > maxHoursSick)
                                            timesheet.SickLeave = maxHoursSick;
                                        else
                                            timesheet.SickLeave = hoursSick;
                                    }
                                }

                                if (leave != null)
                                {
                                    var hoursLeave = decimal.Parse((leave.EndDateTime - leave.StartDateTime).TotalHours.ToString("N2"));
                                    if (hoursLeave != 0)
                                    {
                                        var maxHoursLeave = 7.6m;
                                        maxHoursLeave -= timesheet.OrdTime;

                                        if (hoursLeave > maxHoursLeave)
                                            timesheet.AnnualLeave = maxHoursLeave;
                                        else
                                            timesheet.AnnualLeave = maxHoursLeave;
                                    }
                                }
                            }
                        }
                    }

                    var columnHeaders = 14;

                    foreach (var employee in employeeList)
                    {
                        var timesheets = employeeTimesheets.Where(x => x.EmployeeId == employee);
                        var ordTime = timesheets.Sum(x => x.OrdTime);
                        var weekendTime = timesheets.Sum(x => x.WeekendTime);
                        var annualLeaveTime = timesheets.Sum(x => x.AnnualLeave);
                        var sickLeaveTime = timesheets.Sum(x => x.SickLeave);
                        var publicHoliday = timesheets.Sum(x => x.PublicHol);
                        var workedPublicHoliday = timesheets.Sum(x => x.WorkedPublicHol);
                        var total = ordTime + weekendTime + annualLeaveTime + sickLeaveTime + publicHoliday + workedPublicHoliday;

                        w.WriteLine("Employee Name,Ord Time,Extra Ord Time > 76 hrs,Sat/Sun Ord 1.5,Annual Leave,Sick Leave,Public Hol.,Worked Public Hol. 2.5,for AJA only,OT 1.5,OT 2.0,OT 2.5,TOTAL HOURS,Notes");
                        w.Flush();

                        var extraOrdTime = total - 76;
                        if (extraOrdTime < 0)
                            extraOrdTime = 0;

                        ordTime -= extraOrdTime;

                        var notes = string.Empty;
                        if (sickLeaveTime != 0)
                            notes = "Pay Accrued Sick Leave ";

                        if (annualLeaveTime != 0)
                            notes += "Pay Accrued Annual Leave";


                        w.WriteLine($"{employeeNameLookup[employee]},{ordTime},{extraOrdTime},{weekendTime},{annualLeaveTime},{sickLeaveTime},{publicHoliday},{workedPublicHoliday},,,,,{total},{notes.Trim()}");
                        w.Flush();

                        w.WriteLine(GetBlankLine(columnHeaders));
                        w.Flush();

                        w.WriteLine(GetBlankLine(columnHeaders));
                        w.Flush();
                    }
                }
            }

            return Json("Success");
        }

        private CanegrowersTimesheetPerDay GetEmployeeTimesheet(List<CanegrowersTimesheetPerDay> employeeTimesheets, DateTime date, int employeeId)
        {
            var timesheet = employeeTimesheets.FirstOrDefault(x => x.EmployeeId == employeeId && x.Date == date.Date);
            if (timesheet == null)
            {
                timesheet = new CanegrowersTimesheetPerDay();
                timesheet.EmployeeId = employeeId;
                timesheet.Date = date.Date;
                employeeTimesheets.Add(timesheet);
            }

            return timesheet;
        }

        private string GetBlankLine(int headerLength)
        {
            var newLine = "";
            for (int i = 0; i < headerLength; i++)
                newLine += ",";

            return newLine;
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