using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebPortal.Models
{
    public class PesticideApplicationSprayTimes
    {
        public enum WindDirectionEnum
        {
            North,
            NorthEast,
            NorthWest,
            South,
            SouthEast,
            SouthWest,
            East,
            West
        }

        [Key]
        public int Id { get; set; }
        public int HeaderId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int StartWindSpeed { get; set; }
        public int EndWindSpeed { get; set; }
        public WindDirectionEnum StartWindDirection { get; set; }
        public WindDirectionEnum EndWindDirection { get; set; }
        public int StartTemp { get; set; }
        public int EndTemp { get; set; }
        public decimal StartHumidity { get; set; }
        public decimal EndHumidity { get; set; }

        public virtual PesticideApplicationHeader Header { get; set; }
    }
}