using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class SprayNozzleConfigurationViewModel
    {
        public SprayNozzleConfiguration Config { get; set; }
        public string NozzleA { get; set; }
        public string NozzleB { get; set; }
        public string NozzleC { get; set; }
        public string NozzleD { get; set; }
        public string NozzleE { get; set; }
        public string NozzleF { get; set; }
        public List<SelectListItem> AllSprayNozzles { get; set; }
    }
}
