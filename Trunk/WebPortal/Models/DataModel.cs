using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WebPortal.Models
{
    public class DataModel : DbContext
    {
        public DbSet<TradingEntity> TradingEntity { get; set; }
        public DbSet<Employees> Employees { get; set; }
        public DbSet<Timesheets> Timesheets { get; set; }
        public DbSet<AspNetUsers> AspNetUsers { get; set; }
        public DbSet<AspNetRoles> AspNetRoles { get; set; }
        public DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public DbSet<UserEmployeeAccess> UserEmployeeAccess { get; set; }
        public DbSet<PublicHolidays> PublicHolidays { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PublicHolidays>()
                .HasKey(c => new { c.Date, c.Region });
        }
    }
}
