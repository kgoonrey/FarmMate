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
        public async Task<IActionResult> PesticideApplication()
        {
            var header = new PesticideApplicationHeader();
            await PopulateViewBag();
            return View(header);
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
        public IActionResult AddLine(PesticideApplicationHeader header)
        {
            //TODO: Not working for multiple products
            var model = new PesticideApplicationLines();
            model.Header = header;
            model.HeaderJson = JsonConvert.SerializeObject(header);

            using (var context = new DataModel())
            {
                var productList = context.Products.Select(a => new SelectListItem
                {
                    Value = a.Code,
                    Text = a.Name
                }).ToList();
                ViewBag.Products = productList;
            }

            return PartialView("_AddLine", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddLineContinue(PesticideApplicationLines model)
        {
            await PopulateViewBag();
            model.Header = JsonConvert.DeserializeObject<PesticideApplicationHeader>(model.HeaderJson);

            if (model.Header.Lines == null)
                model.Header.Lines = new List<PesticideApplicationLines>();

            model.Header.Lines.Add(model);

            using (var context = new DataModel())
            {
                foreach(var line in model.Header.Lines)
                {
                    line.ProductTarget = context.Products.FirstOrDefault(x => x.Code == line.Product);
                }

                var user = await _userManager.GetUserAsync(HttpContext.User);
                var userEmployeeAccess = await context.UserEmployeeAccess.Where(x => x.UserId == user.Id).ToListAsync();
                var employees = context.Employees.Where(x => x.Active && x.TradingEntity == model.Header.TradingEntity && userEmployeeAccess.FirstOrDefault(y => y.EmployeeId == x.Id) != null).ToList();
                var roles = context.AspNetRoles.FirstOrDefault(x => x.Name == "Admin");
                var adminUserList = context.AspNetUserRoles.Where(x => x.RoleId == roles.Id).ToList();

                if (adminUserList.FirstOrDefault(x => x.UserId == user.Id) != null)
                    employees = await context.Employees.Where(x => x.Active && x.TradingEntity == model.Header.TradingEntity).ToListAsync();

                var employeesList = employees.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.FirstName} {a.LastName}"
                }).ToList();

                ViewBag.Employees = employeesList;
            }

            return View("PesticideApplication", model.Header);
        }

        [HttpGet]
        public IActionResult EditLine(string id)
        {
            Manufacturers model = null;
            using (var context = new DataModel())
            {
                model = context.Manufacturers.FirstOrDefault(x => x.Code == id);
            }
            return PartialView("_EditManufacturer", model);
        }

        [HttpPost]
        public IActionResult EditLine(string id, Manufacturers model)
        {
            if (model.Description == null || model.Description == string.Empty)
                return RedirectToAction("Index");

            using (var context = new DataModel())
            {
                var manufacturer = context.Manufacturers.FirstOrDefault(x => x.Code == id);
                if (manufacturer == null)
                    return RedirectToAction("Index");

                manufacturer.Description = model.Description;
                context.Update(manufacturer);
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DeleteLine(string id)
        {
            string description = string.Empty;
            using (var context = new DataModel())
            {
                var manufacturer = context.Manufacturers.FirstOrDefault(x => x.Code == id);
                if (manufacturer == null)
                    return RedirectToAction("Index");

                description = manufacturer.Description;
            }
            return PartialView("_DeleteManufacturer", description);
        }

        [HttpPost]
        public IActionResult DeleteLine(string id, Manufacturers model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var manufacturer = context.Manufacturers.FirstOrDefault(x => x.Code == id);
                    if (manufacturer == null)
                        return RedirectToAction("Index");

                    context.Manufacturers.Remove(manufacturer);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }
    }
}