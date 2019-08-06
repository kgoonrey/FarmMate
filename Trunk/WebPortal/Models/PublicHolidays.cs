using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class PublicHolidays
    {
        public DateTime Date { get; set; }
        public string Region { get; set; }
        public string Description { get; set; }
    }
}
