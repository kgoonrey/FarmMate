using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class SprayConfigurationCompetencies
    {
        [Key]
        public int ConfigurationId { get; set; }
        [Key]
        public int EmployeeId { get; set; }
        public bool PrepareAndApplyChemicals { get; set; }
        public bool ControlWeeds { get; set; }
        public bool TransportHandleAndStoreChemicals { get; set; }
        public string Provider { get; set; }
        public string Year { get; set; }
        public string CommercialLicenceNumber { get; set; }

        public virtual SprayNozzleConfiguration ConfigurationTarget { get; set; }
        public virtual Employees EmployeeTarget { get; set; }
    }
}
