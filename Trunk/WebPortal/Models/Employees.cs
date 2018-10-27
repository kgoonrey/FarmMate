using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class Employees
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Occupation { get; set; }
        public int TradingEntity { get; set; }
        public DateTime DefaultStartTime { get; set; }
        public DateTime DefaultEndTime { get; set; }
        public int DefaultBreakAmount { get; set; }
        public bool Active { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> TradingEntities { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Name { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string TradingEntityDescription { get; set; }
    }
}
