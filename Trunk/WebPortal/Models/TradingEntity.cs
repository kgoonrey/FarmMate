using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class TradingEntity
    {
        public TradingEntity()
        {
            PesticideApplications = new HashSet<PesticideApplicationHeader>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public string SMTPHost { get; set; }
        public bool SMTPUseSSL { get; set; }
        public int SMTPPort { get; set; }
        public string SMTPEmailAddress { get; set; }
        public string SMTPPassword { get; set; }
        public string PayablesEmailAddress { get; set; }

        public virtual ICollection<PesticideApplicationHeader> PesticideApplications { get; set; }
    }
}
