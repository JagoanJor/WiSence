using API.Entities;
using API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace API.Services
{
    public interface IReportService
    {
        vReportAbsensi getReportAbsensi(Int64 userID, int bulan, int tahun);
    }
    public class ReportService : IReportService
    {
        public vReportAbsensi getReportAbsensi(Int64 userID, int bulan, int tahun)
        {
            var context = new EFContext();
            try
            {
                var result = new vReportAbsensi();
                string namaBulan = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(bulan);
                var query = String.Format($@"
                    SELECT *
                    FROM
                        vReportAbsensi
                    WHERE
                        UserID = {userID} AND Bulan = '{namaBulan}'");

                var header = context.vReportAbsensis.FromSqlRaw(query).FirstOrDefault();

                var queryList = String.Format($@"
                    SELECT *
                    FROM
                        vReportAbsensiList
                    WHERE
                        UserID = {userID} AND Bulan = '{namaBulan}'");

                var detail = context.vReportAbsensiLists.FromSqlRaw(queryList);
                var detailList = detail.ToList();

                int daysInMonth = DateTime.DaysInMonth(tahun, bulan);
                int hariKerja = 0;

                for (int day = 1; day <= daysInMonth; day++)
                {
                    DateTime currentDate = new DateTime(tahun, bulan, day);

                    if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                        hariKerja++;
                }

                int hariKerjaTanpaLibur = hariKerja - header.Libur;

                result.Nama = header.Nama;
                result.UserID = userID;
                result.Posisi = header.Posisi;
                result.Bulan = header.Bulan;
                result.Kerja = header.Kerja;
                result.Libur = header.Libur;
                result.TotalKerja = $"{header.Kerja} dari {hariKerjaTanpaLibur} hari kerja";
                result.vReportAbsensiLists = detailList;

                return result;
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
