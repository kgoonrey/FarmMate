﻿using System;
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
        public DbSet<SprayConfigurationCompetencies> SprayConfigurationCompetencies { get; set; }
        public DbSet<ProductTypes> ProductTypes { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<ProductMixLines> ProductMixLines { get; set; }
        public DbSet<PesticideApplicationHeader> PesticideApplicationHeader { get; set; }
        public DbSet<PesticideApplicationLines> PesticideApplicationLines { get; set; }
        public DbSet<PesticideApplicationSprayTimes> PesticideApplicationSprayTimes { get; set; }
        public DbSet<ProductGroups> ProductGroups { get; set; }

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

            modelBuilder.Entity<SprayConfigurationCompetencies>()
                .HasKey(c => new { c.Id });

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

                entity.HasOne(d => d.ProductGroupTarget)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.ProductGroup)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Products_ProductGroups");
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

            modelBuilder.Entity<PesticideApplicationLines>(entity =>
            {
                entity.HasOne(d => d.Header)
                    .WithMany(p => p.Lines)
                    .HasForeignKey(d => d.HeaderId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PesticideApplicationLines_PesticideApplicationHeader");

                entity.HasOne(d => d.ProductTarget)
                   .WithMany(p => p.PesticideApplicationLines)
                   .HasForeignKey(d => d.Product)
                   .OnDelete(DeleteBehavior.ClientSetNull)
                   .HasConstraintName("FK_PesticideApplicationLines_Products");
            });

            modelBuilder.Entity<PesticideApplicationSprayTimes>(entity =>
            {
                entity.HasOne(d => d.Header)
                    .WithMany(p => p.Times)
                    .HasForeignKey(d => d.HeaderId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PesticideApplicationSprayTimes_PesticideApplicationHeader");
            });

            modelBuilder.Entity<PesticideApplicationHeader>(entity =>
            {
                entity.HasOne(d => d.TradingEntityTarget)
                    .WithMany(p => p.PesticideApplications)
                    .HasForeignKey(d => d.TradingEntity)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PesticideApplicationHeader_TradingEntity");

                entity.HasOne(d => d.EmployeeTarget)
                    .WithMany(p => p.PesticideApplications)
                    .HasForeignKey(d => d.Employee)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PesticideApplicationHeader_Employees");
            });

            modelBuilder.Entity<SprayConfigurationCompetencies>(entity =>
            {
                entity.HasOne(d => d.EmployeeTarget)
                    .WithMany(p => p.Competencies)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SprayConfigurationCompetencies_Employees");
            });
        }
    }
}
