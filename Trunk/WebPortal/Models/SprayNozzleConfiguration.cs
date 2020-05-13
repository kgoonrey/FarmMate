using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class SprayNozzleConfiguration
    {
        public SprayNozzleConfiguration()
        {
            Competencies = new HashSet<SprayConfigurationCompetencies>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int NozzleAId { get; set; }
        public int NozzleBId { get; set; }
        public int NozzleCId { get; set; }
        public int NozzleDId { get; set; }
        public int NozzleEId { get; set; }
        public int NozzleFId { get; set; }

        public virtual ICollection<SprayConfigurationCompetencies> Competencies { get; set; }
    }
}
