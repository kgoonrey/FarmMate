using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class ProductMixLinesViewModel
    {
        public int Id { get; set; }
        public string HeaderProduct { get; set; }
        public string Product { get; set; }
        public decimal ApplicationRate { get; set; }
        public string RateUOM { get; set; }
        public List<SelectListItem> RateUOMOption { get; set; }
        public List<SelectListItem> AllProducts { get; set; }
    }
}
