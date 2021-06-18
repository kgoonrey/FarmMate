using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class PesticideApplicationHeader
    {
        public enum SprayTypeEnum
        {
            Weed,
            InsectMiteGrub,
            DiseasePathogen,
            ParasiteNematode
        }

        public PesticideApplicationHeader()
        {
            Lines = new HashSet<PesticideApplicationLines>();
            Times = new HashSet<PesticideApplicationSprayTimes>();
        }

        [Key]
        public int Id { get; set; }
        public int TradingEntity { get; set; }
        public string FarmId { get; set; }
        public string BlockNumbers { get; set; }
        public decimal AreaTreated { get; set; }
        public DateTime DateApplied { get; set; }
        public bool Aerial { get; set; }
        public bool Band { get; set; }
        public bool OverCanopy { get; set; }
        public bool UnderCanopy { get; set; }
        public bool DirectSpray { get; set; }
        public bool Shielded { get; set; }
        public bool Hooded { get; set; }
        public bool Boom { get; set; }
        public bool Spot { get; set; }
        public bool Handgun { get; set; }
        public string OtherMethod { get; set; }
        public int NozzleConfiguration { get; set; }
        public bool BOMForecastChecked { get; set; }
        public bool RainForecastIn48Hours { get; set; }
        public int CloudCoverPercentage { get; set; }
        public SprayTypeEnum SprayType { get; set; }
        public string OtherType { get; set; }
        public decimal SystemOperatingPressure { get; set; }
        public int VehicleGroundSpeed { get; set; }
        public int SprayTankVolume { get; set; }
        public decimal SprayerReleaseHeight { get; set; }
        public decimal SprayerApplicationRate { get; set; }
        public bool ProductLabelReadAndUnderstood { get; set; }
        public bool MSDSReadAndUnderstood { get; set; }
        public bool RiskAssessmentUndertaken { get; set; }
        public bool NeighboursNotifiedVerbal { get; set; }
        public bool NeighboursNotifiedWritten { get; set; }
        public bool ACCab { get; set; }
        public bool Overalls { get; set; }
        public bool Goggles { get; set; }
        public bool Respirator { get; set; }
        public bool Hat { get; set; }
        public bool Apron { get; set; }
        public bool Boots { get; set; }
        public bool Gloves { get; set; }
        public int Employee { get; set; }
        public DateTime AuditDateTime { get; set; }
        public string EmployeeSignature { get; set; }
        public bool AuthorisationRequired { get; set; }
        public int AuthorisationEmployee { get; set; }
        public string AuthorisationEmployeeSignature { get; set; }
        public string Notes { get; set; }
        public bool Completed { get; set; }

        public virtual TradingEntity TradingEntityTarget { get; set; }
        public virtual Employees EmployeeTarget { get; set; }
        public virtual ICollection<PesticideApplicationLines> Lines { get; set; }
        public virtual ICollection<PesticideApplicationSprayTimes> Times { get; set; }
    }
}
