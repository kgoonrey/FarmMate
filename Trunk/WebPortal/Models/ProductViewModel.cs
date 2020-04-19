using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class ProductViewModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ActiveConstituents { get; set; }
        public string APVMANumber { get; set; }
        public string Type { get; set; }
        public bool ERAProduct { get; set; }
        public int TankSize { get; set; }
        public List<SelectListItem> AllTypes { get; set; }
    }
}
