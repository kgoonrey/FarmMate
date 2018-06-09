using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class Timesheets
    {
        public Guid Id { get; set; }
        public int Employee { get; set; }
        public int TradingEntity { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal BreakAmount { get; set; }
        public string Notes { get; set; }
    }
}
