using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class AspNetUserRoles
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
}
