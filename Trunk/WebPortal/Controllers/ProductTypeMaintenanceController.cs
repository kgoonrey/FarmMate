using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPortal.Models;

namespace WebPortal.Controllers
{
    public class ProductTypeMaintenanceController : Controller
    {
        public IActionResult Index()
        {
            List<ProductTypes> model = new List<ProductTypes>();

            using (var context = new DataModel())
            {
                model = context.ProductTypes.ToList();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AddProductType()
        {
            var model = new ProductTypes();
            return PartialView("_AddProductType", model);
        }

        [HttpPost]
        public IActionResult AddProductType(ProductTypes model)
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
        public IActionResult EditProductType(string id)
        {
            ProductTypes model = null;
            using (var context = new DataModel())
            {
                model = context.ProductTypes.FirstOrDefault(x=> x.Code == id);
            }
            return PartialView("_EditProductType", model);
        }

        [HttpPost]
        public IActionResult EditProductType(string id, ProductTypes model)
        {
            if (model.Description == null || model.Description == string.Empty)
                return RedirectToAction("Index");

            using (var context = new DataModel())
            {
                var productType = context.ProductTypes.FirstOrDefault(x => x.Code == id);
                if (productType == null)
                    return RedirectToAction("Index");

                productType.Description = model.Description;
                context.Update(productType);
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DeleteProductType(string id)
        {
            string description = string.Empty;
            using (var context = new DataModel())
            {
                var productType = context.ProductTypes.FirstOrDefault(x => x.Code == id);
                if (productType == null)
                    return RedirectToAction("Index");

                description = productType.Description;
            }
            return PartialView("_DeleteProductType", description);
        }

        [HttpPost]
        public IActionResult DeleteProductType(string id, ProductTypes model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var productType = context.ProductTypes.FirstOrDefault(x => x.Code == id);
                    if (productType == null)
                        return RedirectToAction("Index");

                    context.ProductTypes.Remove(productType);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }
    }
}