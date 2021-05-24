using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public enum RateUOMEnum
    {
        KilogramPerHa,
        LitrePerHa,
        GramPerHa,
        MilliliterPerHa,
        MilliliterPer100L
    }

    public class ProductMixLines
    {
        [Key]
        public int Id { get; set; }
        public string HeaderProduct { get; set; }
        public string Product { get; set; }
        public decimal ApplicationRate { get; set; }
        public RateUOMEnum RateUOM { get; set; }

        [NotMapped]
        public string RateUOMString { get; set; }

        public virtual Products HeaderProductTarget { get; set; }
        public virtual Products ProductTarget { get; set; }
    }    
}
