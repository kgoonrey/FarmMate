using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class UserEmployeeAccess
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int EmployeeId { get; set; }
    }
}
