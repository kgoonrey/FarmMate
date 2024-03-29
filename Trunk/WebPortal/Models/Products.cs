﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class Products
    {
        public Products()
        {
            ProductMixHeaderLines = new HashSet<ProductMixLines>();
            ProductMixLines = new HashSet<ProductMixLines>();
            PesticideApplicationLines = new HashSet<PesticideApplicationLines>();
        }

        [Key]
        public string Code { get; set; }
        public string Name { get; set; }
        public string ActiveConstituents { get; set; }
        public string APVMANumber { get; set; }
        public string Type { get; set; }
        public bool ERAProduct { get; set; }
        public int TankSize { get; set; }
        public int ProductGroup { get; set; }

        public virtual ProductTypes TypeTarget { get; set; }
        public virtual ProductGroups ProductGroupTarget { get; set; }
        public virtual ICollection<ProductMixLines> ProductMixHeaderLines { get; set; }
        public virtual ICollection<ProductMixLines> ProductMixLines { get; set; }
        public virtual ICollection<PesticideApplicationLines> PesticideApplicationLines { get; set; }
    }
}
