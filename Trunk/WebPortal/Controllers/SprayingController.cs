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

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
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
            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines), 0, string.Empty));
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
                ViewBag.TradingEntitys = tradingEntity;
                ViewBag.Employees = new List<SelectListItem>();
            }
        }

        [HttpPost]
        public IActionResult Submit([Bind(Prefix = "Item1")] PesticideApplicationHeader header, [Bind(Prefix = "Item2")] string lineJson, [Bind(Prefix = "Item4")] string type)
        {
            if (type == "Add")
                return AddLine(header, lineJson);
            if (type.StartsWith("Edit"))
                return EditLine(int.Parse(type.Split(":")[1]), header, lineJson);
            if (type.StartsWith("Delete"))
                return DeleteLine(int.Parse(type.Split(":")[1]), header, lineJson);

            return PesticideApplication(header).Result;
        }

        [HttpGet]
        public IActionResult AddLine(PesticideApplicationHeader header, string lineJson)
        {
            header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);

            var line = new PesticideApplicationLines();
            line.Id = header.Lines.Any() ? header.Lines.Max(x => x.Id) + 1 : 0; 
            line.HeaderJson = JsonConvert.SerializeObject(header);

            using (var context = new DataModel())
            {
                var productList = context.Products.Select(a => new SelectListItem
                {
                    Value = a.Code,
                    Text = a.Name
                }).ToList();
                ViewBag.Products = productList;
            }

            return PartialView("_AddLine", line);
        }

        [HttpPost]
        public async Task<IActionResult> AddLineContinue(PesticideApplicationLines model)
        {
            await PopulateViewBag();
            var header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            model.Header = null;
            header.Lines.Add(model);

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

            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines), 2, string.Empty));
        }

        [HttpGet]
        public IActionResult EditLine(int id, [Bind(Prefix = "Item1")] PesticideApplicationHeader header, [Bind(Prefix = "Item2")] string lineJson)
        {
            header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);

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
            }

            return PartialView("_EditLine", line);
        }

        [HttpPost]
        public async Task<IActionResult> EditLineContinue(PesticideApplicationLines model)
        {
            await PopulateViewBag();
            var header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            model.Header = null;
            var line = header.Lines.FirstOrDefault(x => x.Id == model.Id);

            line.Product = model.Product;
            line.Quantity = model.Quantity;

            using (var context = new DataModel())
            {
                foreach (var row in header.Lines)
                {
                    row.ProductTarget = context.Products.FirstOrDefault(x => x.Code == row.Product);
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

            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines), 2, string.Empty));
        }

        [HttpGet]
        public IActionResult DeleteLine(int id, [Bind(Prefix = "Item1")] PesticideApplicationHeader header, [Bind(Prefix = "Item2")] string lineJson)
        {
            header.Lines = JsonConvert.DeserializeObject<List<PesticideApplicationLines>>(lineJson);

            var line = header.Lines.FirstOrDefault(x => x.Id == id);
            line.HeaderJson = JsonConvert.SerializeObject(header);

            return PartialView("_DeleteLine", line);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLineContinue(PesticideApplicationLines model)
        {
            await PopulateViewBag();
            var header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            model.Header = null;
            var line = header.Lines.FirstOrDefault(x => x.Id == model.Id);

            header.Lines.Remove(line);

            using (var context = new DataModel())
            {
                foreach (var row in header.Lines)
                {
                    row.ProductTarget = context.Products.FirstOrDefault(x => x.Code == row.Product);
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

            return View("PesticideApplication", Tuple.Create(header, JsonConvert.SerializeObject(header.Lines), 2, string.Empty));
        }
    }
}