using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using KuaforRandevuSistemi.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace KuaforRandevuSistemi.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Entity'lerimiz için DbSet tanımlamaları
        public DbSet<Salon> Salons { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<EmployeeService> EmployeeServices { get; set; }
        public DbSet<WorkingHours> WorkingHours { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AIStyleSuggestion> AIStyleSuggestions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Salon ilişkileri
            modelBuilder.Entity<Salon>(entity =>
            {
                entity.HasMany(s => s.Services)
                    .WithOne(s => s.Salon)
                    .HasForeignKey(s => s.SalonId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(s => s.Employees)
                    .WithOne(e => e.Salon)
                    .HasForeignKey(e => e.SalonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Service ilişkileri
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasMany(s => s.EmployeeServices)
                    .WithOne(es => es.Service)
                    .HasForeignKey(es => es.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(s => s.Appointments)
                    .WithOne(a => a.Service)
                    .HasForeignKey(a => a.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Employee ilişkileri
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasMany(e => e.EmployeeServices)
                    .WithOne(es => es.Employee)
                    .HasForeignKey(es => es.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.WorkingHours)
                    .WithOne(w => w.Employee)
                    .HasForeignKey(w => w.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Appointments)
                    .WithOne(a => a.Employee)
                    .HasForeignKey(a => a.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // User ilişkileri
            modelBuilder.Entity<AppUser>(entity =>  // User yerine AppUser
            {
                entity.HasMany(u => u.AIStyleSuggestions)
                    .WithOne(ai => ai.User)
                    .HasForeignKey(ai => ai.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(u => u.Appointments)
                    .WithOne(a => a.Customer)
                    .HasForeignKey(a => a.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Alan konfigürasyonları
            modelBuilder.Entity<Salon>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<AppUser>(entity =>  // User yerine AppUser
            {
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            });
        }
    }
}