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
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Wifi> Wifis { get; set; }
        public DbSet<DailyTask> DailyTasks { get; set; }
        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<Cuti> Cutis { get; set; }
    }
}