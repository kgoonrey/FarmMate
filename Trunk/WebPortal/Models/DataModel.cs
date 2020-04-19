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
        public DbSet<SprayNozzles> SprayNozzles { get; set; }
        public DbSet<Manufacturers> Manufacturers { get; set; }
        public DbSet<SprayNozzleConfiguration> SprayNozzleConfiguration { get; set; }
        public DbSet<ProductTypes> ProductTypes { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<ProductMixLines> ProductMixLines { get; set; }

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

            modelBuilder.Entity<SprayNozzles>(entity =>
            {
                entity.HasOne(d => d.ManufacturerTarget)
                    .WithMany(p => p.SprayNozzles)
                    .HasForeignKey(d => d.Manufacturer)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SprayNozzles_Manufacturers");
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.HasOne(d => d.TypeTarget)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Products_ProductTypes");
            });

            modelBuilder.Entity<ProductMixLines>(entity =>
            {
                entity.HasOne(d => d.HeaderProductTarget)
                    .WithMany(p => p.ProductMixHeaderLines)
                    .HasForeignKey(d => d.HeaderProduct)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductMixLinesHeader_Products");

                entity.HasOne(d => d.ProductTarget)
                    .WithMany(p => p.ProductMixLines)
                    .HasForeignKey(d => d.Product)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductMixLines_Products");
            });
        }
    }
}
