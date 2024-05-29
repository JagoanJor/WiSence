using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System.Data;
using System.Diagnostics;

using API.Entities;

namespace API.Helpers
{
    public class EFContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(Utils.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Password> Passwords { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RoleDetail> RoleDetails { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Position> Positions { get; set; }
        //public DbSet<Wifi> Wifis { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<DailyTask> DailyTasks { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        
        public DbSet<vReportAbsensi> vReportAbsensis { get; set; }
        public DbSet<vReportAbsensiList> vReportAbsensiLists { get; set; }
        public DbSet<vReportAbsensiListPerTahun> vReportAbsensiListPerTahuns { get; set; }
        public DbSet<vReportCuti> vReportCutis { get; set; }
        public DbSet<vReportCutiList> vReportCutiLists { get; set; }
        public DbSet<vReportCutiPerTahun> vReportCutiPerTahuns { get; set; }
    }
}