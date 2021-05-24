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
                model = context.SprayConfigurationCompetencies.Where(x => x.EmployeeId == id).ToList();
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

            model.Id = null;

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
        public IActionResult EditCompetencie(int id)
        {
            SprayConfigurationCompetencies model = null;
            using (var context = new DataModel())
            {
                model = context.SprayConfigurationCompetencies.FirstOrDefault(x=> x.Id == id);
            }
            return PartialView("_EditCompetencie", model);
        }

        [HttpPost]
        public IActionResult EditCompetencie(SprayConfigurationCompetencies model)
        {
            if (model.EmployeeId == 0)
                return RedirectToAction("Index");

            if (model.Provider == null)
                model.Provider = string.Empty;

            if (model.Year == null)
                model.Year = string.Empty;

            if (model.CommercialLicenceNumber == null)
                model.CommercialLicenceNumber = string.Empty;

            using (var context = new DataModel())
            {
                context.Update(model);
                context.SaveChanges();
            }

            return RedirectToAction("Index", new { id = model.EmployeeId });
        }

        [HttpGet]
        public IActionResult DeleteCompetencie(int id)
        {
            SprayConfigurationCompetencies model = null;
            using (var context = new DataModel())
            {
                model = context.SprayConfigurationCompetencies.FirstOrDefault(x => x.Id == id);
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