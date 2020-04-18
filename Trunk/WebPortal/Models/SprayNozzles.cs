using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class SprayNozzles
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }

        public virtual Manufacturers ManufacturerTarget { get; set; }
    }
}
