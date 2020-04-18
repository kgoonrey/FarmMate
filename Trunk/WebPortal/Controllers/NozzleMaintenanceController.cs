using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPortal.Models;

namespace WebPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NozzleMaintenanceController : Controller
    {
        [HttpGet]
        public IActionResult Index(int tab)
        {
            List<SprayNozzles> model = new List<SprayNozzles>();
            List<SprayNozzleConfigurationViewModel> model2 = new List<SprayNozzleConfigurationViewModel>();

            using (var context = new DataModel())
            {
                model = context.SprayNozzles.Include(x => x.ManufacturerTarget).ToList();
                var configs = context.SprayNozzleConfiguration.ToList();

                foreach (var config in configs)
                {
                    var nozzleA = model.FirstOrDefault(x => x.Id == config.NozzleAId);
                    var nozzleB = model.FirstOrDefault(x => x.Id == config.NozzleBId);
                    var nozzleC = model.FirstOrDefault(x => x.Id == config.NozzleCId);
                    var nozzleD = model.FirstOrDefault(x => x.Id == config.NozzleDId);
                    var nozzleE = model.FirstOrDefault(x => x.Id == config.NozzleEId);
                    var nozzleF = model.FirstOrDefault(x => x.Id == config.NozzleFId);

                    var newModel = new SprayNozzleConfigurationViewModel();
                    newModel.Config = config;
                    newModel.NozzleA = nozzleA?.Name ?? string.Empty;
                    newModel.NozzleB = nozzleB?.Name ?? string.Empty;
                    newModel.NozzleC = nozzleC?.Name ?? string.Empty;
                    newModel.NozzleD = nozzleD?.Name ?? string.Empty;
                    newModel.NozzleE = nozzleE?.Name ?? string.Empty;
                    newModel.NozzleF = nozzleF?.Name ?? string.Empty;
                    model2.Add(newModel);
                }
            }

            var newTest = Tuple.Create(model, model2, tab);
            return View(newTest);
        }

        [HttpGet]
        public IActionResult AddNozzle()
        {
            var model = new SprayNozzleViewModel();
            using (var context = new DataModel())
            {
                model.AllManufacturers = context.Manufacturers.Select(r => new SelectListItem
                {
                    Text = r.Description,
                    Value = r.Code
                }).ToList();
            }
            return PartialView("_AddNozzle", model);
        }

        [HttpPost]
        public IActionResult AddNozzle(SprayNozzleViewModel model)
        {
            if (model.Name == null || model.Name == string.Empty)
                ModelState.AddModelError(string.Empty, "Name missing");

            if (model.Manufacturer == null || model.Manufacturer == "Please select")
                ModelState.AddModelError(string.Empty, "Manufacturer missing");

            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var newNozzle = new SprayNozzles();
                    newNozzle.Name = model.Name;
                    newNozzle.ManufacturerTarget = context.Manufacturers.FirstOrDefault(x => x.Code == model.Manufacturer);

                    context.Add(newNozzle);
                    context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            using (var context = new DataModel())
            {
                model.AllManufacturers = context.Manufacturers.Select(r => new SelectListItem
                {
                    Text = r.Description,
                    Value = r.Code
                }).ToList();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditNozzle(int id)
        {
            var model = new SprayNozzleViewModel();
            using (var context = new DataModel())
            {
                var nozzle = context.SprayNozzles.Include(x => x.ManufacturerTarget).FirstOrDefault(x => x.Id == id);
                if (nozzle == null)
                    return RedirectToAction("Index");

                model.Name = nozzle.Name;
                model.Manufacturer = nozzle.Manufacturer;
                model.AllManufacturers = context.Manufacturers.Select(r => new SelectListItem
                {
                    Text = r.Description,
                    Value = r.Code
                }).ToList();
            }
            return PartialView("_EditNozzle", model);
        }

        [HttpPost]
        public IActionResult EditNozzle(int id, SprayNozzleViewModel model)
        {
            if (model.Name == null || model.Name == string.Empty)
                ModelState.AddModelError(string.Empty, "Name missing");

            if (model.Manufacturer == null || model.Manufacturer == "Please select")
                ModelState.AddModelError(string.Empty, "Manufacturer missing");

            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var nozzle = context.SprayNozzles.FirstOrDefault(x => x.Id == id);
                    if (nozzle == null)
                        return RedirectToAction("Index");

                    nozzle.Name = model.Name;
                    nozzle.ManufacturerTarget = context.Manufacturers.FirstOrDefault(x => x.Code == model.Manufacturer);

                    context.Update(nozzle);
                    context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            using (var context = new DataModel())
            {
                model.AllManufacturers = context.Manufacturers.Select(r => new SelectListItem
                {
                    Text = r.Description,
                    Value = r.Code
                }).ToList();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DeleteNozzle(int id)
        {
            string name = string.Empty;
            using (var context = new DataModel())
            {
                var nozzle = context.SprayNozzles.Include(x => x.ManufacturerTarget).FirstOrDefault(x => x.Id == id);
                if (nozzle == null)
                    return RedirectToAction("Index");

                name = nozzle.Name;
            }
            return PartialView("_DeleteNozzle", name);
        }

        [HttpPost]
        public IActionResult DeleteNozzle(int id, SprayNozzleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var nozzle = context.SprayNozzles.Where(x => x.Id == id).FirstOrDefault();
                    context.SprayNozzles.Remove(nozzle);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddConfiguration()
        {
            var model = new SprayNozzleConfigurationViewModel();
            using (var context = new DataModel())
            {
                model.AllSprayNozzles = context.SprayNozzles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                }).ToList();
            }
            return PartialView("_AddConfiguration", model);
        }

        [HttpPost]
        public IActionResult AddConfiguration(SprayNozzleConfigurationViewModel model)
        {
            using (var context = new DataModel())
            {
                model.AllSprayNozzles = context.SprayNozzles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                }).ToList();
            }

            if (model.Config.Name == null || model.Config.Name == string.Empty)
                return RedirectToAction("Index", new { tab = 1 });

            if (model.Config.NozzleAId == 0 && model.Config.NozzleBId == 0 && model.Config.NozzleCId == 0 && model.Config.NozzleDId == 0 && model.Config.NozzleEId == 0 && model.Config.NozzleFId == 0)
                return RedirectToAction("Index", new { tab = 1 });

            using (var context = new DataModel())
            {
                context.Add(model.Config);
                context.SaveChanges();
            }

            return RedirectToAction("Index", new { tab = 1 });
        }

        [HttpGet]
        public IActionResult EditConfiguration(int id)
        {
            var model = new SprayNozzleConfigurationViewModel();
            using (var context = new DataModel())
            {
                var configs = context.SprayNozzleConfiguration.ToList();
                var nozzles = context.SprayNozzles.Include(x => x.ManufacturerTarget).ToList();

                foreach (var config in configs)
                {
                    var nozzleA = nozzles.FirstOrDefault(x => x.Id == config.NozzleAId);
                    var nozzleB = nozzles.FirstOrDefault(x => x.Id == config.NozzleBId);
                    var nozzleC = nozzles.FirstOrDefault(x => x.Id == config.NozzleCId);
                    var nozzleD = nozzles.FirstOrDefault(x => x.Id == config.NozzleDId);
                    var nozzleE = nozzles.FirstOrDefault(x => x.Id == config.NozzleEId);
                    var nozzleF = nozzles.FirstOrDefault(x => x.Id == config.NozzleFId);

                    model.Config = config;
                    model.NozzleA = nozzleA?.Name ?? string.Empty;
                    model.NozzleB = nozzleB?.Name ?? string.Empty;
                    model.NozzleC = nozzleC?.Name ?? string.Empty;
                    model.NozzleD = nozzleD?.Name ?? string.Empty;
                    model.NozzleE = nozzleE?.Name ?? string.Empty;
                    model.NozzleF = nozzleF?.Name ?? string.Empty;
                }
            }

            using (var context = new DataModel())
            {
                model.AllSprayNozzles = context.SprayNozzles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                }).ToList();
            }

            return PartialView("_EditConfiguration", model);
        }

        [HttpPost]
        public IActionResult EditConfiguration(int id, SprayNozzleConfigurationViewModel model)
        {
            using (var context = new DataModel())
            {
                model.AllSprayNozzles = context.SprayNozzles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                }).ToList();
            }

            if (model.Config.Name == null || model.Config.Name == string.Empty)
                return RedirectToAction("Index", new { tab = 1 });

            if (model.Config.NozzleAId == 0 && model.Config.NozzleBId == 0 && model.Config.NozzleCId == 0 && model.Config.NozzleDId == 0 && model.Config.NozzleEId == 0 && model.Config.NozzleFId == 0)
                return RedirectToAction("Index", new { tab = 1 });

            using (var context = new DataModel())
            {
                var existingConfig = context.SprayNozzleConfiguration.FirstOrDefault(x => x.Id == id);
                existingConfig.Name = model.Config.Name;
                existingConfig.NozzleAId = model.Config.NozzleAId;
                existingConfig.NozzleBId = model.Config.NozzleBId;
                existingConfig.NozzleCId = model.Config.NozzleCId;
                existingConfig.NozzleDId = model.Config.NozzleDId;
                existingConfig.NozzleEId = model.Config.NozzleEId;
                existingConfig.NozzleFId = model.Config.NozzleFId;

                context.Update(existingConfig);
                context.SaveChanges();
            }

            return RedirectToAction("Index", new { tab = 1 });
        }

        [HttpGet]
        public IActionResult DeleteConfiguration(int id)
        {
            string name = string.Empty;
            using (var context = new DataModel())
            {
                var config = context.SprayNozzleConfiguration.FirstOrDefault(x => x.Id == id);
                if (config == null)
                    return RedirectToAction("Index", new { tab = 1 });

                name = config.Name;
            }
            return PartialView("_DeleteConfiguration", name);
        }

        [HttpPost]
        public IActionResult DeleteConfiguration(int id, SprayNozzleConfigurationViewModel model)
        {
            using (var context = new DataModel())
            {
                var config = context.SprayNozzleConfiguration.Where(x => x.Id == id).FirstOrDefault();
                context.SprayNozzleConfiguration.Remove(config);
                context.SaveChanges();
            }
            return RedirectToAction("Index", new { tab = 1 });
        }
    }
}