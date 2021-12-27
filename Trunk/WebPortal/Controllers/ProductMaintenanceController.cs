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
    public class ProductMaintenanceController : Controller
    {
        public IActionResult Index()
        {
            List<Products> model = new List<Products>();

            using (var context = new DataModel())
            {
                model = context.Products.Include(x=> x.TypeTarget).ToList();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            ProductViewModel viewModel = new ProductViewModel();
            using (var context = new DataModel())
            {
                viewModel.AllTypes = context.ProductTypes.Select(r => new SelectListItem
                {
                    Text = r.Description,
                    Value = r.Code
                }).ToList();

                viewModel.ProductGroups = context.ProductGroups.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                }).ToList();
            }
            return PartialView("_AddProduct", viewModel);
        }

        [HttpPost]
        public IActionResult AddProduct(ProductViewModel viewModel)
        {
            if (viewModel.Code == null || viewModel.Code == string.Empty)
                ModelState.AddModelError(string.Empty, "Code missing");

            if (viewModel.Name == null || viewModel.Name == string.Empty)
                ModelState.AddModelError(string.Empty, "Name missing");

            if (viewModel.Type == null || viewModel.Type == "Please select")
                ModelState.AddModelError(string.Empty, "Type missing");

            if (!int.TryParse(viewModel?.ProductGroup ?? "", out var productGroup) || viewModel.ProductGroup == null || viewModel.ProductGroup == "Please select")
                ModelState.AddModelError(string.Empty, "Product group missing");

            if (viewModel.ActiveConstituents == null)
                viewModel.ActiveConstituents = string.Empty;

            if (viewModel.APVMANumber == null)
                viewModel.APVMANumber = string.Empty;

            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var model = new Products();
                    model.Code = viewModel.Code;
                    model.Name = viewModel.Name;
                    model.ActiveConstituents = viewModel.ActiveConstituents;
                    model.APVMANumber = viewModel.APVMANumber;
                    model.Type = viewModel.Type;
                    model.ERAProduct = viewModel.ERAProduct;
                    model.TankSize = viewModel.TankSize;
                    model.ProductGroup = productGroup;

                    context.Add(model);
                    context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditProduct(string id)
        {
            ProductViewModel viewModel = new ProductViewModel();
            using (var context = new DataModel())
            {
                var model = context.Products.FirstOrDefault(x => x.Code == id);
                viewModel.Code = model.Code;
                viewModel.Name = model.Name;
                viewModel.ActiveConstituents = model.ActiveConstituents;
                viewModel.APVMANumber = model.APVMANumber;
                viewModel.Type = model.Type;
                viewModel.ERAProduct = model.ERAProduct;
                viewModel.TankSize = model.TankSize;
                viewModel.ProductGroup = model.ProductGroup.ToString();

                viewModel.AllTypes = context.ProductTypes.Select(r => new SelectListItem
                {
                    Text = r.Description,
                    Value = r.Code
                }).ToList();

                viewModel.ProductGroups = context.ProductGroups.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Id.ToString()
                }).ToList();
            }
            return PartialView("_EditProduct", viewModel);
        }

        [HttpPost]
        public IActionResult EditProduct(string id, ProductViewModel viewModel)
        {
            if (viewModel.Name == null || viewModel.Name == string.Empty)
                return RedirectToAction("Index");

            if (viewModel.Type == null || viewModel.Type == "Please select")
                return RedirectToAction("Index");

            if (!int.TryParse(viewModel?.ProductGroup ?? "", out var productGroup) || viewModel.ProductGroup == null || viewModel.ProductGroup == "Please select")
                return RedirectToAction("Index");

            if (viewModel.ActiveConstituents == null)
                viewModel.ActiveConstituents = string.Empty;

            if (viewModel.APVMANumber == null)
                viewModel.APVMANumber = string.Empty;

            using (var context = new DataModel())
            {
                var product = context.Products.FirstOrDefault(x => x.Code == id);
                if (product == null)
                    return RedirectToAction("Index");

                product.Name = viewModel.Name;
                product.ActiveConstituents = viewModel.ActiveConstituents;
                product.APVMANumber = viewModel.APVMANumber;
                product.Type = viewModel.Type;
                product.ERAProduct = viewModel.ERAProduct;
                product.TankSize = viewModel.TankSize;
                product.ProductGroup = productGroup;

                context.Update(product);
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DeleteProduct(string id)
        {
            string name = string.Empty;
            using (var context = new DataModel())
            {
                var product = context.Products.FirstOrDefault(x => x.Code == id);
                if (product == null)
                    return RedirectToAction("Index");

                name = product.Name;
            }
            return PartialView("_DeleteProduct", name);
        }

        [HttpPost]
        public IActionResult DeleteProduct(string id, Products model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var product = context.Products.FirstOrDefault(x => x.Code == id);
                    if (product == null)
                        return RedirectToAction("Index");

                    context.Products.Remove(product);
                    context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult MixProduct(string id)
        {
            List<ProductMixLines> productMixLines = null;
            var parentProduct = new Products();

            using (var context = new DataModel())
            {
                productMixLines = context.ProductMixLines.Include(x=> x.ProductTarget).Where(x => x.HeaderProduct == id).ToList();
                parentProduct = context.Products.FirstOrDefault(x => x.Code == id);
            }

            foreach(var row in productMixLines)
            {
                row.RateUOMString = EnumConversion(row.RateUOM);
            }

            var newTuple = Tuple.Create(parentProduct, productMixLines);
            return View("Mix", newTuple);
        }

        private string EnumConversion(RateUOMEnum value)
        {
            if (value == RateUOMEnum.KilogramPerHa)
                return "kg/ha";
            if (value == RateUOMEnum.LitrePerHa)
                return "L/ha";
            if (value == RateUOMEnum.GramPerHa)
                return "g/ha";
            if (value == RateUOMEnum.MilliliterPerHa)
                return "mL/ha";
            if (value == RateUOMEnum.MilliliterPer100L)
                return "mL/100L";
            if (value == RateUOMEnum.Each)
                return "Each";

            return string.Empty;
        }

        [HttpGet]
        public IActionResult AddProductMix(string id)
        {
            ProductMixLinesViewModel viewModel = new ProductMixLinesViewModel();
            viewModel.HeaderProduct = id;

            using (var context = new DataModel())
            {
                viewModel.AllProducts = context.Products.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Code
                }).ToList();

                var rateUOM = new List<SelectListItem>();
                rateUOM.Add(new SelectListItem() { Text = "kg/ha", Value = ((int)RateUOMEnum.KilogramPerHa).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "L/ha", Value = ((int)RateUOMEnum.LitrePerHa).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "g/ha", Value = ((int)RateUOMEnum.GramPerHa).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "mL/ha", Value = ((int)RateUOMEnum.MilliliterPerHa).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "mL/100L", Value = ((int)RateUOMEnum.MilliliterPer100L).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "Each", Value = ((int)RateUOMEnum.Each).ToString() });
                viewModel.RateUOMOption = rateUOM;
            }
            return PartialView("_AddProductMix", viewModel);
        }

        [HttpPost]
        public IActionResult AddProductMix(ProductMixLinesViewModel viewModel)
        {
            if (viewModel.Product == null || viewModel.Product == "Please select")
                return RedirectToAction("MixProduct", new { id = viewModel.HeaderProduct });

            if (viewModel.RateUOM == null || viewModel.RateUOM == "Please select")
                return RedirectToAction("MixProduct", new { id = viewModel.HeaderProduct });

            using (var context = new DataModel())
            {
                var model = new ProductMixLines();
                model.HeaderProduct = viewModel.HeaderProduct;
                model.Product = viewModel.Product;
                model.ApplicationRate = viewModel.ApplicationRate;
                model.RateUOM =  (RateUOMEnum) int.Parse(viewModel.RateUOM);

                context.Add(model);
                context.SaveChanges();
            }

            return RedirectToAction("MixProduct", new { id = viewModel.HeaderProduct });
        }

        [HttpGet]
        public IActionResult EditProductMix(int id)
        {
            ProductMixLinesViewModel viewModel = new ProductMixLinesViewModel();

            using (var context = new DataModel())
            {
                var model = context.ProductMixLines.FirstOrDefault(x => x.Id == id);
                viewModel.Id = model.Id;
                viewModel.HeaderProduct = model.HeaderProduct;
                viewModel.Product = model.Product;
                viewModel.ApplicationRate = model.ApplicationRate;
                viewModel.RateUOM = ((int)model.RateUOM).ToString();

                viewModel.AllProducts = context.Products.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Code
                }).ToList();

                var rateUOM = new List<SelectListItem>();
                rateUOM.Add(new SelectListItem() { Text = "kg/ha", Value = ((int)RateUOMEnum.KilogramPerHa).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "L/ha", Value = ((int)RateUOMEnum.LitrePerHa).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "g/ha", Value = ((int)RateUOMEnum.GramPerHa).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "mL/ha", Value = ((int)RateUOMEnum.MilliliterPerHa).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "mL/100L", Value = ((int)RateUOMEnum.MilliliterPer100L).ToString() });
                rateUOM.Add(new SelectListItem() { Text = "Each", Value = ((int)RateUOMEnum.Each).ToString() });
                viewModel.RateUOMOption = rateUOM;
            }
            return PartialView("_EditProductMix", viewModel);
        }

        [HttpPost]
        public IActionResult EditProductMix(ProductMixLinesViewModel viewModel)
        {
            if (viewModel.Product == null || viewModel.Product == "Please select")
                ModelState.AddModelError(string.Empty, "Product missing");

            if (viewModel.RateUOM == null || viewModel.RateUOM == "Please select")
                ModelState.AddModelError(string.Empty, "UOM missing");

            var header = string.Empty;

            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var model = context.ProductMixLines.FirstOrDefault(x => x.Id == viewModel.Id);
                    header = model.HeaderProduct;
                    model.Product = viewModel.Product;
                    model.ApplicationRate = viewModel.ApplicationRate;
                    model.RateUOM = (RateUOMEnum)int.Parse(viewModel.RateUOM);

                    context.Update(model);
                    context.SaveChanges();
                }
            }
            else
            {
                using (var context = new DataModel())
                {
                    var model = context.ProductMixLines.FirstOrDefault(x => x.Id == viewModel.Id);
                    header = model.HeaderProduct;
                }
            }

            return RedirectToAction("MixProduct", new { id = header });
        }

        [HttpGet]
        public IActionResult DeleteProductMix(int id)
        {
            string name = string.Empty;
            using (var context = new DataModel())
            {
                var productMixLine = context.ProductMixLines.Include(x=> x.ProductTarget).FirstOrDefault(x => x.Id == id);
                if (productMixLine == null)
                    return RedirectToAction("Index");

                name = productMixLine.ProductTarget.Name;
            }
            return PartialView("_DeleteProductMix", name);
        }

        [HttpPost]
        public IActionResult DeleteProductMix(int id, ProductMixLines model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new DataModel())
                {
                    var productMixLine = context.ProductMixLines.FirstOrDefault(x => x.Id == id);
                    if (productMixLine == null)
                        return RedirectToAction("Index");

                    var header = productMixLine.HeaderProduct;
                    context.ProductMixLines.Remove(productMixLine);
                    context.SaveChanges();
                    return RedirectToAction("MixProduct", new { id = header });
                }
            }

            return RedirectToAction("Index");
        }

    }
}