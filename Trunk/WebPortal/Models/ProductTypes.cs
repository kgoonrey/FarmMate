using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class ProductTypes
    {
        public ProductTypes()
        {
            Products = new HashSet<Products>();
        }

        [Key]
        public string Code { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Products> Products { get; set; }
    }
}
