using API.Entities;
using API.Helpers;
using System.Diagnostics;
using System.Linq;
using System;
using API.Responses;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public interface IDashboardService
    {
        Task<int> TotalKaryawanAsync();
        Task<int> TotalHadirAsync();
        Task<int> TotalPositionAsync();
        Task<int> TotalDivisionAsync();
        Task<(int ontime, int terlambat, int cuti, int absen)> GetJumlahKehadiranHariIniAsync(User user);
        Task<bool> CheckCutiAsync();
    }

    public class DashboardService : IDashboardService
    {
        public async Task<int> TotalKaryawanAsync()
        {
            using (var context = new EFContext())
            {
                try
                {
                    var users = await context.Users.Where(x => x.IsDeleted != true && x.IsAdmin != true).ToListAsync();
                    if (users == null || users.Count == 0) return 0;

                    var roles = await context.Roles.Where(x => x.IsDeleted != true && x.Name != "Admin").Select(x => x.RoleID).ToListAsync();
                    if (roles == null || roles.Count == 0) throw new Exception("Data role tidak ada!");

                    int count = users.Count(x => roles.Contains(x.RoleID));
                    return count;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    if (ex.StackTrace != null)
                        Trace.WriteLine(ex.StackTrace);

                    throw;
                }
            }
        }

        public async Task<int> TotalHadirAsync()
        {
            using (var context = new EFContext())
            {
                try
                {
                    int count = await context.Attendances
                        .Where(x => x.IsDeleted != true && x.ClockIn.Value.Date == DateTime.UtcNow.Date && x.Status != "Cuti")
                        .CountAsync();
                    return count;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    if (ex.StackTrace != null)
                        Trace.WriteLine(ex.StackTrace);

                    throw;
                }
            }
        }

        public async Task<int> TotalPositionAsync()
        {
            using (var context = new EFContext())
            {
                try
                {
                    int count = await context.Positions.Where(x => x.IsDeleted != true).CountAsync();
                    return count;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    if (ex.StackTrace != null)
                        Trace.WriteLine(ex.StackTrace);

                    throw;
                }
            }
        }

        public async Task<int> TotalDivisionAsync()
        {
            using (var context = new EFContext())
            {
                try
                {
                    int count = await context.Divisions.Where(x => x.IsDeleted != true).CountAsync();
                    return count;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    if (ex.StackTrace != null)
                        Trace.WriteLine(ex.StackTrace);

                    throw;
                }
            }
        }

        public async Task<(int ontime, int terlambat, int cuti, int absen)> GetJumlahKehadiranHariIniAsync(User user)
        {
            using (var context = new EFContext())
            {
                try
                {
                    var userInCompany = await context.Users.Where(x => x.CompanyID == user.CompanyID && x.IsDeleted != true && x.IsAdmin != true).ToListAsync();
                    if (userInCompany == null || userInCompany.Count == 0)
                        throw new Exception("Please set user's company!");

                    int totalUser = userInCompany.Count();

                    int ontime = await context.Attendances
                        .Where(x => x.IsDeleted != true && x.Status == "Ontime" && userInCompany.Any(u => u.UserID == x.UserID) && x.Date.Value.Date == DateTime.UtcNow.Date)
                        .CountAsync();

                    int terlambat = await context.Attendances
                        .Where(x => x.IsDeleted != true && x.Status == "Terlambat" && userInCompany.Any(u => u.UserID == x.UserID) && x.Date.Value.Date == DateTime.UtcNow.Date)
                        .CountAsync();

                    int cuti = await context.Attendances
                        .Where(x => x.IsDeleted != true && x.Status == "Cuti" && userInCompany.Any(u => u.UserID == x.UserID) && x.Date.Value.Date == DateTime.UtcNow.Date)
                        .CountAsync();

                    int absen = totalUser - ontime - terlambat - cuti;

                    return (ontime, terlambat, cuti, absen);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    if (ex.StackTrace != null)
                        Trace.WriteLine(ex.StackTrace);

                    throw;
                }
            }
        }

        public async Task<bool> CheckCutiAsync()
        {
            using (var context = new EFContext())
            {
                try
                {
                    int count = await context.Leaves.CountAsync(x => x.Status == "Menunggu" && x.IsDeleted != true);
                    return count > 0;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    if (ex.StackTrace != null)
                        Trace.WriteLine(ex.StackTrace);

                    throw;
                }
            }
        }
    }
}
