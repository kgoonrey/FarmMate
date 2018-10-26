using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class LinkEmployeeViewModel
    {
        public string UserId { get; set; }
        public List<Employees> SelectedList { get; set; }
        public List<Employees> OptionList { get; set; }
    }

    public class SaveEmployeeViewModel
    {
        public int EmployeeId { get; set; }
        public string UserId { get; set; }
    }
}
