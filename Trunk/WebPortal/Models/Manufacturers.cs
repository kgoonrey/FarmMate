using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class Manufacturers
    {
        public Manufacturers()
        {
            SprayNozzles = new HashSet<SprayNozzles>();
        }

        [Key]
        public string Code { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }

        public virtual ICollection<SprayNozzles> SprayNozzles { get; set; }
    }
}
