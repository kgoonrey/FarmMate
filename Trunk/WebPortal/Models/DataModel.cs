using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebPortal.Models
{
    public class DataModel : DbContext
    {
        public DbSet<TradingEntity> TradingEntity { get; set; }
        public DbSet<Employees> Employees { get; set; }
        public DbSet<Timesheets> Timesheets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(local)\SQLExpress;Initial Catalog=FarmMate;Integrated Security=True");
        }
    }
}
