using AnalysisCallUser._01_Domain.Core.Entities;
using AnalysisCallUser._02_Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AnalysisCallUser._02_Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<CallDetail> CallDetails { get; set; }
        public DbSet<CallType> CallTypes { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<UserLoginHistory> LoginHistories { get; set; }
        public DbSet<FilterHistory> FilterHistories { get; set; }
        public DbSet<ExportHistory> ExportHistories { get; set; }
        public DbSet<DashboardWidget> DashboardWidgets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CallDetailConfiguration());
            modelBuilder.ApplyConfiguration(new CallTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CityConfiguration());
            modelBuilder.ApplyConfiguration(new CountryConfiguration());
            modelBuilder.ApplyConfiguration(new OperatorConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
            modelBuilder.ApplyConfiguration(new UserLoginHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new FilterHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new ExportHistoryConfiguration());
        }
    }
}
