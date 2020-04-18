using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPortal.Models;

namespace WebPortal.Controllers
{
    public class ManufacturerMaintenanceController : Controller
    {
        public IActionResult Index()
        {
            List<Manufacturers> model = new List<Manufacturers>();

            using (var context = new DataModel())
            {
                model = context.Manufacturers.ToList();
            }

            return View(model);
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
        public IActionResult EditManufacturer(string id)
        {
            Manufacturers model = null;
            using (var context = new DataModel())
            {
                model = context.Manufacturers.FirstOrDefault(x=> x.Code == id);
            }
            return PartialView("_EditManufacturer", model);
        }

        [HttpPost]
        public IActionResult EditManufacturer(string id, Manufacturers model)
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
        public IActionResult DeleteManufacturer(string id)
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
        public IActionResult DeleteManufacturer(string id, Manufacturers model)
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