﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using WebPortal.Data;
using WebPortal.Models;

namespace WebPortal.Controllers
{
    [Authorize]
    public class SprayingController : Controller
    {
        private IHostingEnvironment _hostingEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public SprayingController(IHostingEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager)
        {
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }

        [Authorize]
        public IActionResult Index()
        {
            using (var context = new DataModel())
            {
                return View(context.PesticideApplicationHeader.Include(x=> x.TradingEntityTarget).Include(x=> x.EmployeeTarget).OrderByDescending(x=> x.DateApplied).ToList());
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Maintenance()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PesticideApplication(PesticideApplicationHeader header)
        {
            await PopulateViewBag();
            header.DateApplied = DateTime.Today;
            header.TradingEntity = 1; //TODO Setting
            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines), JsonConvert.SerializeObject(header.Times), 0, string.Empty));
        }

        private async Task PopulateViewBag()
        {
            using (var context = new DataModel())
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userEmployeeAccess = await context.UserEmployeeAccess.Where(x => x.UserId == user.Id).ToListAsync();
                var employees = await context.Employees.Where(x => x.Active && userEmployeeAccess.FirstOrDefault(y => y.EmployeeId == x.Id) != null).ToListAsync();

                var roles = await context.AspNetRoles.FirstOrDefaultAsync(x => x.Name == "Admin");
                var adminUserList = await context.AspNetUserRoles.Where(x => x.RoleId == roles.Id).ToListAsync();

                if (adminUserList.FirstOrDefault(x => x.UserId == user.Id) != null)
                    employees = await context.Employees.Where(x => x.Active).ToListAsync();

                var tradingEntity = await context.TradingEntity.Where(x => employees.FirstOrDefault(y => y.TradingEntity == x.Id) != null).Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Description
                }).ToListAsync();

                var nozzleConfis = await context.SprayNozzleConfiguration.OrderByDescending(x=> x.Id).Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToListAsync();

                var employeesList = employees.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.FirstName} {a.LastName}"
                }).ToList();

                ViewBag.TradingEntitys = tradingEntity;
                ViewBag.Employees = employeesList;
                ViewBag.NozzleConfigurations = nozzleConfis;
            }

            var sprayTypes = new List<SelectListItem>();

            foreach (PesticideApplicationHeader.SprayTypeEnum item in Enum.GetValues(typeof(PesticideApplicationHeader.SprayTypeEnum)))
            {
                var description = string.Empty;
                if (item == PesticideApplicationHeader.SprayTypeEnum.Weed)
                    description = "Weed";
                else if (item == PesticideApplicationHeader.SprayTypeEnum.InsectMiteGrub)
                    description = "Insect/Mite/Grub";
                else if (item == PesticideApplicationHeader.SprayTypeEnum.DiseasePathogen)
                    description = "Disease/Pathogen";
                else if (item == PesticideApplicationHeader.SprayTypeEnum.ParasiteNematode)
                    description = "Parasite/Nematode";

                sprayTypes.Add(new SelectListItem() { Value = ((int)item).ToString(), Text = description });
            }

            ViewBag.SprayTypes = sprayTypes;

        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public IActionResult Submit([Bind(Prefix = "Item1")] PesticideApplicationHeader header, [Bind(Prefix = "Item2")] string lineJson, [Bind(Prefix = "Item3")] string timeJson, [Bind(Prefix = "Item5")] string type)
        {
            if (type == "LineAdd")
                return AddLine(header, lineJson, timeJson);
            if (type.StartsWith("LineEdit"))
                return EditLine(Guid.Parse(type.Split(":")[1]), header, lineJson, timeJson);
            if (type.StartsWith("LineDelete"))
                return DeleteLine(Guid.Parse(type.Split(":")[1]), header, lineJson, timeJson);

            if (type == "TimeAdd")
                return AddTime(header, lineJson, timeJson);
            if (type.StartsWith("TimeEdit"))
                return EditTime(Guid.Parse(type.Split(":")[1]), header, lineJson, timeJson);
            if (type.StartsWith("TimeDelete"))
                return DeleteTime(Guid.Parse(type.Split(":")[1]), header, lineJson, timeJson);

            if (type == "Saving")
            {
                header.Completed = true;
                header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);
                header.Times = JsonConvert.DeserializeObject<List<PesticideApplicationSprayTimes>>(timeJson);
                return Saving(header);
            }

            return PesticideApplication(header).Result;
        }

        [HttpGet]
        public IActionResult AddLine(PesticideApplicationHeader header, string lineJson, string timeJson)
        {
            header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);
            header.Times = JsonConvert.DeserializeObject<List<PesticideApplicationSprayTimes>>(timeJson);

            var line = new PesticideApplicationLines();
            line.Id = Guid.NewGuid();
            line.HeaderJson = JsonConvert.SerializeObject(header);

            using (var context = new DataModel())
            {
                var productList = context.Products.Select(a => new SelectListItem
                {
                    Value = a.Code,
                    Text = a.Name
                }).ToList();
                ViewBag.Products = productList;

                var productGroupList = context.ProductGroups.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToList();
                ViewBag.ProductGroupList = productGroupList;
            }

            var rateUOM = new List<SelectListItem>();
            rateUOM.Add(new SelectListItem() { Text = "kg/ha", Value = ((int)RateUOMEnum.KilogramPerHa).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "L/ha", Value = ((int)RateUOMEnum.LitrePerHa).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "g/ha", Value = ((int)RateUOMEnum.GramPerHa).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "mL/ha", Value = ((int)RateUOMEnum.MilliliterPerHa).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "mL/100L", Value = ((int)RateUOMEnum.MilliliterPer100L).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "Each", Value = ((int)RateUOMEnum.Each).ToString() });
            line.RateUOMOption = rateUOM;
            line.ApplicationRate = RateUOMEnum.LitrePerHa;

            return PartialView("_AddLine", line);
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> AddLineContinue(PesticideApplicationLines model)
        {
            await PopulateViewBag();
            var header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            model.Header = null;
            if (model.Product != "Please select")
                header.Lines.Add(model);

            await PopulateOldData(header);

            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonConvert.SerializeObject(header.Times, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 2, string.Empty));
        }

        [HttpGet]
        public IActionResult EditLine(Guid id, PesticideApplicationHeader header, string lineJson, string timeJson)
        {
            header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);
            header.Times = JsonConvert.DeserializeObject<List<PesticideApplicationSprayTimes>>(timeJson);

            var line = header.Lines.FirstOrDefault(x => x.Id == id);
            line.HeaderJson = JsonConvert.SerializeObject(header);

            using (var context = new DataModel())
            {
                var productList = context.Products.Select(a => new SelectListItem
                {
                    Value = a.Code,
                    Text = a.Name
                }).ToList();
                ViewBag.Products = productList;

                var productGroupList = context.ProductGroups.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToList();
                ViewBag.ProductGroupList = productGroupList;
            }

            var rateUOM = new List<SelectListItem>();
            rateUOM.Add(new SelectListItem() { Text = "kg/ha", Value = ((int)RateUOMEnum.KilogramPerHa).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "L/ha", Value = ((int)RateUOMEnum.LitrePerHa).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "g/ha", Value = ((int)RateUOMEnum.GramPerHa).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "mL/ha", Value = ((int)RateUOMEnum.MilliliterPerHa).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "mL/100L", Value = ((int)RateUOMEnum.MilliliterPer100L).ToString() });
            rateUOM.Add(new SelectListItem() { Text = "Each", Value = ((int)RateUOMEnum.Each).ToString() });
            line.RateUOMOption = rateUOM;

            return PartialView("_EditLine", line);
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> EditLineContinue(PesticideApplicationLines model)
        {
            await PopulateViewBag();
            var header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            model.Header = null;
            var line = header.Lines.FirstOrDefault(x => x.Id == model.Id);

            line.Product = model.Product;
            line.Quantity = model.Quantity;
            line.ApplicationRate = model.ApplicationRate;
            await PopulateOldData(header);

            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonConvert.SerializeObject(header.Times, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 2, string.Empty));
        }

        [HttpGet]
        public IActionResult DeleteLine(Guid id, PesticideApplicationHeader header, string lineJson, string timeJson)
        {
            header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);
            header.Times = JsonConvert.DeserializeObject<List<PesticideApplicationSprayTimes>>(timeJson);

            var line = header.Lines.FirstOrDefault(x => x.Id == id);
            line.HeaderJson = JsonConvert.SerializeObject(header);

            return PartialView("_DeleteLine", line);
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> DeleteLineContinue(PesticideApplicationLines model)
        {
            await PopulateViewBag();
            var header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            model.Header = null;
            var line = header.Lines.FirstOrDefault(x => x.Id == model.Id);

            header.Lines.Remove(line);
            await PopulateOldData(header);

            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonConvert.SerializeObject(header.Times, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 2, string.Empty));
        }

        [HttpGet]
        public IActionResult AddTime(PesticideApplicationHeader header, string lineJson, string timeJson)
        {
            header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);
            header.Times = JsonConvert.DeserializeObject<List<PesticideApplicationSprayTimes>>(timeJson);

            var time = new PesticideApplicationSprayTimes();
            time.Id = Guid.NewGuid();
            time.HeaderJson = JsonConvert.SerializeObject(header);

            var current = DateTime.Now;

            current = current.AddSeconds(-current.Second);
            current = current.AddMilliseconds(-current.Millisecond);

            if (current.Minute < 10)
                current = current.AddMinutes(-current.Minute);
            else if (current.Minute < 20)
                current = current.AddMinutes(-current.Minute).AddMinutes(15);
            else if (current.Minute < 40)
                current = current.AddMinutes(-current.Minute).AddMinutes(30);
            else if (current.Minute < 55)
                current = current.AddMinutes(-current.Minute).AddMinutes(45);
            else
                current = current.AddMinutes(-current.Minute).AddHours(1);

            time.StartTime = current;
            time.EndTime = current.AddHours(3);

            var windDirection = new List<SelectListItem>();

            foreach(PesticideApplicationSprayTimes.WindDirectionEnum item in Enum.GetValues(typeof(PesticideApplicationSprayTimes.WindDirectionEnum)))
            {
                windDirection.Add(new SelectListItem() { Value = ((int)item).ToString(), Text = item.ToString() });
            }

            ViewBag.WindDirection = windDirection;
            time.StartWindDirection = PesticideApplicationSprayTimes.WindDirectionEnum.SouthEast;
            time.EndWindDirection = PesticideApplicationSprayTimes.WindDirectionEnum.SouthEast;
            return PartialView("_AddTime", time);
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> AddTimeContinue(PesticideApplicationSprayTimes model)
        {
            await PopulateViewBag();
            var header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            model.Header = null;
            header.Times.Add(model);
            await PopulateOldData(header);

            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonConvert.SerializeObject(header.Times, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 3, string.Empty));
        }

        [HttpGet]
        public IActionResult EditTime(Guid id, PesticideApplicationHeader header, string lineJson, string timeJson)
        {
            header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);
            header.Times = JsonConvert.DeserializeObject<List<PesticideApplicationSprayTimes>>(timeJson);

            var time = header.Times.FirstOrDefault(x => x.Id == id);
            time.HeaderJson = JsonConvert.SerializeObject(header);

            var windDirection = new List<SelectListItem>();

            foreach (PesticideApplicationSprayTimes.WindDirectionEnum item in Enum.GetValues(typeof(PesticideApplicationSprayTimes.WindDirectionEnum)))
            {
                windDirection.Add(new SelectListItem() { Value = ((int)item).ToString(), Text = item.ToString() });
            }

            ViewBag.WindDirection = windDirection;
            return PartialView("_EditTime", time);
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> EditTimeContinue(PesticideApplicationSprayTimes model)
        {
            await PopulateViewBag();
            var header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            model.Header = null;
            var time = header.Times.FirstOrDefault(x => x.Id == model.Id);

            time.StartTime = model.StartTime;
            time.EndTime = model.EndTime;
            time.StartWindSpeed = model.StartWindSpeed;
            time.EndWindSpeed = model.EndWindSpeed;
            time.StartWindDirection = model.StartWindDirection;
            time.EndWindDirection = model.EndWindDirection;
            time.StartTemp = model.StartTemp;
            time.EndTemp = model.EndTemp;
            time.StartHumidity = model.StartHumidity;
            time.EndHumidity = model.EndHumidity;

            await PopulateOldData(header);
            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonConvert.SerializeObject(header.Times, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 3, string.Empty));
        }

        [HttpGet]
        public IActionResult DeleteTime(Guid id, PesticideApplicationHeader header, string lineJson, string timeJson)
        {
            header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);
            header.Times = JsonConvert.DeserializeObject<List<PesticideApplicationSprayTimes>>(timeJson);

            var time = header.Times.FirstOrDefault(x => x.Id == id);
            time.HeaderJson = JsonConvert.SerializeObject(header);

            return PartialView("_DeleteTime", time);
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> DeleteTimeContinue(PesticideApplicationSprayTimes model)
        {
            await PopulateViewBag();
            var header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            model.Header = null;
            var line = header.Times.FirstOrDefault(x => x.Id == model.Id);

            header.Times.Remove(line);
            await PopulateOldData(header);

            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonConvert.SerializeObject(header.Times, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 3, string.Empty));
        }

        private async Task PopulateOldData(PesticideApplicationHeader header)
        {
            Saving(header);

            using (var context = new DataModel())
            {
                foreach (var line in header.Lines)
                {
                    line.ProductTarget = context.Products.FirstOrDefault(x => x.Code == line.Product);
                }

                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userEmployeeAccess = await context.UserEmployeeAccess.Where(x => x.UserId == user.Id).ToListAsync();
                var employees = context.Employees.Where(x => x.Active && x.TradingEntity == header.TradingEntity && userEmployeeAccess.FirstOrDefault(y => y.EmployeeId == x.Id) != null).ToList();
                var roles = context.AspNetRoles.FirstOrDefault(x => x.Name == "Admin");
                var adminUserList = context.AspNetUserRoles.Where(x => x.RoleId == roles.Id).ToList();

                if (adminUserList.FirstOrDefault(x => x.UserId == user.Id) != null)
                    employees = await context.Employees.Where(x => x.Active && x.TradingEntity == header.TradingEntity).ToListAsync();

                var employeesList = employees.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.FirstName} {a.LastName}"
                }).ToList();

                ViewBag.Employees = employeesList;
            }
        }

        [HttpGet]
        public IActionResult Saving(PesticideApplicationHeader header)
        {
            if (header.Notes == null)
                header.Notes = string.Empty;

            if (header.OtherType == null)
                header.OtherType = string.Empty;

            if (header.OtherMethod == null)
                header.OtherMethod = string.Empty;

            if (header.BlockNumbers == null)
                header.BlockNumbers = string.Empty;

            if (header.FarmId == null)
                header.FarmId = string.Empty;

            if (header.AuthorisationEmployeeSignature == null)
                header.AuthorisationEmployeeSignature = string.Empty;

            if (header.EmployeeSignature == null)
                header.EmployeeSignature = string.Empty;

            foreach (var line in header.Lines)
                line.ProductTarget = null;

            using (var context = new DataModel())
            {
                var currentLines = header.Lines.ToList();
                var currentTimes = header.Times.ToList();

                var existingHeader = context.PesticideApplicationHeader.FirstOrDefault(x => x.Id == header.Id);
                var existingLines = context.PesticideApplicationLines.Where(x => x.HeaderId == header.Id);
                var existingTimes = context.PesticideApplicationSprayTimes.Where(x => x.HeaderId == header.Id);

                if (existingHeader != null)
                {
                    var date = existingHeader.AuditDateTime;
                    ext.CopyAllTo(header, existingHeader);

                    existingHeader.AuditDateTime = date;

                    context.Update(existingHeader);

                    foreach (var line in header.Lines)
                    {
                        if (existingLines.Any(x => x.Id == line.Id))
                        {
                            context.Update(line);
                        }
                        else
                        {
                            context.PesticideApplicationLines.Add(line);
                        }
                    }

                    foreach(var line in existingLines)
                    {
                        if (!currentLines.Any(x => x.Id == line.Id))
                            context.Remove(line);
                    }

                    foreach (var line in header.Times)
                    {
                        if (existingTimes.Any(x => x.Id == line.Id))
                        {
                            context.Update(line);
                        }
                        else
                        {
                            context.PesticideApplicationSprayTimes.Add(line);
                        }
                    }

                    foreach (var line in existingTimes)
                    {
                        if (!currentTimes.Any(x => x.Id == line.Id))
                            context.Remove(line);
                    }
                }
                else
                {
                    var competency = context.SprayConfigurationCompetencies.FirstOrDefault(x => x.EmployeeId == header.Employee && (x.ExpiryDate == null || x.ExpiryDate > header.DateApplied));
                    if (competency == null)
                        header.AuthorisationRequired = true;
                    else
                        header.AuthorisationRequired = false;

                    header.AuditDateTime = DateTime.Now;
                    context.Add(header);
                }

                context.SaveChanges();
            }

            System.Threading.Thread.Sleep(2000);
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> EditApplication(int id)
        {
            PesticideApplicationHeader header;
            using (var context = new DataModel())
            {
                header = context.PesticideApplicationHeader.Include(x=> x.Lines).Include(x=> x.Times).FirstOrDefault(x => x.Id == id);
            }

            foreach (var line in header.Lines)
                line.Header = null;

            foreach (var line in header.Times)
                line.Header = null;

            await PopulateViewBag();
            await PopulateOldData(header);

            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), JsonConvert.SerializeObject(header.Times, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 0, string.Empty));
        }

        [HttpGet]
        public IActionResult DeleteApplication(int id)
        {
            using (var context = new DataModel())
            {
                var header = context.PesticideApplicationHeader.FirstOrDefault(x => x.Id == id);
                if (header == null)
                    return RedirectToAction("Index");
            }
            return PartialView("_DeleteApplication");
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public IActionResult DeleteApplication(int id, PesticideApplicationHeader model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var header = context.PesticideApplicationHeader.Include(x=> x.Lines).Include(x=> x.Times).Where(x => x.Id == id).FirstOrDefault();
                    context.PesticideApplicationHeader.Remove(header);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        [Route("Spraying/CanAuthoriseApplication")]
        public async Task<IActionResult> CanAuthoriseApplication([FromBody]PesticideApplicationHeader applicationHeader)
        {
            using (var context = new DataModel())
            {
                var application = context.PesticideApplicationHeader.FirstOrDefault(x => x.Id == applicationHeader.Id && x.AuthorisationRequired);
                if (application == null)
                {
                    return Ok("Application already authorised");
                }

                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userEmployeeAccess = await context.UserEmployeeAccess.Where(x => x.UserId == user.Id).ToListAsync();
                var employees = await context.Employees.Where(x => x.Active && userEmployeeAccess.FirstOrDefault(y => y.EmployeeId == x.Id) != null).ToListAsync();

                var roles = await context.AspNetRoles.FirstOrDefaultAsync(x => x.Name == "Admin");
                var adminUserList = await context.AspNetUserRoles.Where(x => x.RoleId == roles.Id).ToListAsync();

                if (adminUserList.FirstOrDefault(x => x.UserId == user.Id) != null)
                    employees = await context.Employees.Where(x => x.Active).ToListAsync();

                
                var employeeList = new List<SelectListItem>();
                foreach (var employee in employees)
                {
                    var competency = context.SprayConfigurationCompetencies.FirstOrDefault(x => x.EmployeeId == employee.Id && (x.ExpiryDate == null || x.ExpiryDate > application.DateApplied));
                    if (competency != null)
                        employeeList.Add(new SelectListItem() { Text = employee.FirstName + " " + employee.LastName, Value = employee.Id.ToString() });
                }

                if (employeeList.Any())
                {
                    ViewBag.Employees = employeeList;

                    if (employeeList.Count == 1)
                        application.AuthorisationEmployee = int.Parse(employeeList[0].Value);

                    return PartialView("_AuthoriseApplication", application);
                }

                return Ok("You don't have the competency qualification to authorise this application.");
            }
        }

        [DisableRequestSizeLimit]
        [HttpPost]
        public IActionResult AuthoriseApplication(PesticideApplicationHeader model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var header = context.PesticideApplicationHeader.FirstOrDefault(x => x.Id == model.Id);
                    header.AuthorisationRequired = false;
                    header.AuthorisationEmployee = model.AuthorisationEmployee;
                    header.AuthorisationEmployeeSignature = model.AuthorisationEmployeeSignature;

                    context.Update(header);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }
        
    }

    public static class ext
    {
        public static void CopyAllTo<T>(this T source, T target)
        {
            var type = typeof(T);
            foreach (var sourceProperty in type.GetProperties())
            {
                var targetProperty = type.GetProperty(sourceProperty.Name);
                targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
            }
            foreach (var sourceField in type.GetFields())
            {
                var targetField = type.GetField(sourceField.Name);
                targetField.SetValue(target, sourceField.GetValue(source));
            }
        }
    }
}