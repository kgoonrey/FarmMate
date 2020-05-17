using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPortal.Models;

namespace WebPortal.Controllers
{
    public class EmployeeCompetenciesController : Controller
    {
        public IActionResult Index(int id)
        {
            var model = new List<SprayConfigurationCompetencies>();
            var employeeName = string.Empty;
            using (var context = new DataModel())
            {
                var employee = context.Employees.FirstOrDefault(x => x.Id == id);
                model = context.SprayConfigurationCompetencies.Include(x => x.ConfigurationTarget).Where(x => x.EmployeeId == id).ToList();
                employeeName = employee.FirstName + " " + employee.LastName;
            }

            return View(Tuple.Create(model, employeeName, id));
        }

        [HttpGet]
        public IActionResult AddManufacturer()
        {
            var model = new Manufacturers();
            return PartialView("_AddManufacturer", model);
        }

        [HttpPost]
        public IActionResult AddManufacturer(Manufacturers model)
        {
            if (model.Code == null || model.Code == string.Empty)
                ModelState.AddModelError(string.Empty, "Name missing");

            if (model.Description == null || model.Description == string.Empty)
                ModelState.AddModelError(string.Empty, "Description missing");

            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    context.Add(model);
                    context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddCompetencie(int id)
        {
            var model = new SprayConfigurationCompetencies();
            model.EmployeeId = id;

            using (var context = new DataModel())
            {
                var nozzleConfis = context.SprayNozzleConfiguration.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToList();

                ViewBag.NozzleConfigurations = nozzleConfis;
            }

            return PartialView("_AddCompetencie", model);
        }

        [HttpPost]
        public IActionResult AddCompetencie(SprayConfigurationCompetencies model)
        {
            if (model.Provider == null)
                model.Provider = string.Empty;

            if (model.Year == null)
                model.Year = string.Empty;

            if (model.CommercialLicenceNumber == null)
                model.CommercialLicenceNumber = string.Empty;

            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    context.Add(model);
                    context.SaveChanges();
                }
            }

            return RedirectToAction("Index", new { id = model.EmployeeId });
        }

        [HttpGet]
        public IActionResult EditCompetencie(string id)
        {
            var key = id.Split(":");

            SprayConfigurationCompetencies model = null;
            using (var context = new DataModel())
            {
                model = context.SprayConfigurationCompetencies.FirstOrDefault(x=> x.ConfigurationId == int.Parse(key[0]) && x.EmployeeId == int.Parse(key[1]));

                var nozzleConfis = context.SprayNozzleConfiguration.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToList();

                ViewBag.NozzleConfigurations = nozzleConfis;
            }
            return PartialView("_EditCompetencie", model);
        }

        [HttpPost]
        public IActionResult EditCompetencie(SprayConfigurationCompetencies model)
        {
            if (model.ConfigurationId == 0 || model.EmployeeId == 0)
                return RedirectToAction("Index");

            using (var context = new DataModel())
            {
                context.Update(model);
                context.SaveChanges();
            }

            return RedirectToAction("Index", new { id = model.EmployeeId });
        }

        [HttpGet]
        public IActionResult DeleteCompetencie(string id)
        {
            var key = id.Split(":");

            SprayConfigurationCompetencies model = null;
            using (var context = new DataModel())
            {
                model = context.SprayConfigurationCompetencies.FirstOrDefault(x => x.ConfigurationId == int.Parse(key[0]) && x.EmployeeId == int.Parse(key[1]));
            }
            return PartialView("_DeleteCompetencie", model);
        }

        [HttpPost]
        public IActionResult DeleteManufacturer(SprayConfigurationCompetencies model)
        {
            var employeeId = model.EmployeeId;

            using (var context = new DataModel())
            {
                context.SprayConfigurationCompetencies.Remove(model);
                context.SaveChanges();
            }
            return RedirectToAction("Index", new { id = employeeId });
        }
    }
}