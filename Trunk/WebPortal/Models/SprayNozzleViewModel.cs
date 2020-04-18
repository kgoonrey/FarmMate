using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class SprayNozzleViewModel
    {
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public List<SelectListItem> AllManufacturers { get; set; }
    }
}
