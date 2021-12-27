using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class PesticideApplicationLines
    {
        [Key]
        public Guid Id { get; set; }
        public int HeaderId { get; set; }
        public string Product { get; set; }
        public RateUOMEnum ApplicationRate { get; set; }
        public decimal Quantity { get; set; }

        public virtual PesticideApplicationHeader Header { get; set; }
        public virtual Products ProductTarget { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string HeaderJson { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<SelectListItem> RateUOMOption { get; set; }
    }
}
