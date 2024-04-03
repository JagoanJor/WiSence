using API.Entities;
using API.Helpers;
using System.Diagnostics;
using System.Linq;
using System;

namespace API.Services
{
    public interface IDashboardService
    {
        int TotalKaryawan();
        int TotalHadir();
        int TotalPosition();
        int TotalDivision();
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

                foreach (var r in role)
                {
                    obj += user.Count(x => roleIDs.Contains(x.RoleID));
                }

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
                var obj = context.Attendances.Where(x => x.IsDeleted != true && x.ClockIn.Value.Date == DateTime.Now.Date).Count();
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
    }
}
