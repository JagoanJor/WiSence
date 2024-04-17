using API.Entities;
using API.Helpers;
using System.Diagnostics;
using System.Linq;
using System;
using API.Responses;

namespace API.Services
{
    public interface IDashboardService
    {
        int TotalKaryawan();
        int TotalHadir();
        int TotalPosition();
        int TotalDivision();
        (int ontime, int terlambat, int cuti, int absen) GetJumlahKehadiranHariIni(User user);
        bool CheckCuti();
    }
    public class DashboardService : IDashboardService
    {
        public int TotalKaryawan()
        {
            var context = new EFContext();
            try
            {
                var user = context.Users.Where(x => x.IsDeleted != true);
                if (user == null) return 0;

                var role = context.Roles.Where(x => x.IsDeleted != true && x.Name != "Admin");
                if (role == null) throw new Exception("Data role tidak ada!");

                var roleIDs = role.Select(x => x.ID).ToList();

                int obj = 0;
                obj += user.Count(x => roleIDs.Contains(x.RoleID));

                return obj;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                context.Dispose();
            }
        }

        public int TotalHadir()
        {
            var context = new EFContext();
            try
            {
                var obj = context.Attendances.Where(x => x.IsDeleted != true && x.ClockIn.Value.Date == DateTime.Now.Date && x.Status != "Cuti").Count();
                if (obj == null) return 0;

                return obj;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                context.Dispose();
            }
        }

        public int TotalPosition()
        {
            var context = new EFContext();
            try
            {
                var obj = context.Positions.Where(x => x.IsDeleted != true).Count();
                if (obj == null) return 0;

                return obj;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                context.Dispose();
            }
        }

        public int TotalDivision()
        {
            var context = new EFContext();
            try
            {
                var obj = context.Divisions.Where(x => x.IsDeleted != true).Count();
                if (obj == null) return 0;

                return obj;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                context.Dispose();
            }
        }

        public (int ontime, int terlambat, int cuti, int absen) GetJumlahKehadiranHariIni(User user)
        {
            var context = new EFContext();
            try
            {
                var userInCompany = context.Users.Where(x => x.CompanyID == user.CompanyID && x.IsDeleted != true && x.IsAdmin != true);
                if (userInCompany == null)
                    throw new Exception("Please set user's company!");

                var totalUser = userInCompany.Count();
                if (totalUser == 0) return (0, 0, 0, 0);

                var ontime = context.Attendances.Where(x => x.IsDeleted != true && x.Status == "Ontime" && userInCompany.Any(u => u.ID == x.UserID) && x.Date.Value.Date == DateTime.Now.Date).Count();
                if (ontime == null) ontime = 0;

                var terlambat = context.Attendances.Where(x => x.IsDeleted != true && x.Status == "Terlambat" && userInCompany.Any(u => u.ID == x.UserID) && x.Date.Value.Date == DateTime.Now.Date).Count();
                if (terlambat == null) terlambat = 0;

                var cuti = context.Attendances.Where(x => x.IsDeleted != true && x.Status == "Cuti" && userInCompany.Any(u => u.ID == x.UserID) && x.Date.Value.Date == DateTime.Now.Date).Count();
                if (cuti == null) cuti = 0;

                var absen = (((totalUser - ontime) - terlambat) - cuti);

                return (ontime, terlambat, cuti, absen);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                context.Dispose();
            }
        }

        public bool CheckCuti()
        {
            var context = new EFContext();
            try
            {
                var obj = context.Cutis.Count(x => x.Status == "Menunggu" && x.IsDeleted != true);
                if (obj == 0)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}
