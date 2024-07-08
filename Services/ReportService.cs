using API.Entities;
using API.Helpers;
using API.Responses;
using Microsoft.EntityFrameworkCore;
using NativeWifi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IReportService
    {
        vReportAbsensi getReportAbsensi(Int64 userID, int bulan, int tahun);
        ReportAbsensiPerTahunResponse getReportAbsensiPerTahun(int tahun);
        ReportCutiResponse getReportCuti(Int64 userID, int bulan, int tahun);
        ReportCutiPerTahunResponse getReportCutiPerTahun(int tahun);
    }
    public class ReportService : IReportService
    {
        public vReportAbsensi getReportAbsensi(Int64 userID, int bulan, int tahun)
        {
            var context = new EFContext();
            try
            {
                // Check user's attendance
                CheckAttendanceAsync(userID);

                // Get report function
                var result = new vReportAbsensi();
                string namaBulan = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(bulan);
                var query = String.Format($@"
                    SELECT *
                    FROM
                        vReportAbsensi
                    WHERE
                        UserID = {userID}");

                var header = context.vReportAbsensis.FromSqlRaw(query).FirstOrDefault();
                if (header == null)
                    return null;

                header.Kerja = context.Attendances.Where(x => x.UserID == header.UserID && x.IsDeleted != true && x.Date.Value.Month == bulan && x.Date.Value.Year == tahun && x.Status != "Cuti" && x.Status != "Absen").Count();
                header.Libur = context.Calendars.Where(x => x.IsDeleted != true && x.Holiday.Month == bulan && x.Holiday.Year == tahun)
                                .ToList()
                                .Where(x => x.Holiday.DayOfWeek != DayOfWeek.Saturday && x.Holiday.DayOfWeek != DayOfWeek.Sunday)
                                .Count();
                header.Periode = $"{bulan} {tahun}";

                var queryList = String.Format($@"
                    SELECT *
                    FROM
                        vReportAbsensiList
                    WHERE
                        UserID = {userID} AND Periode = '{namaBulan} {tahun}'");

                var detail = context.vReportAbsensiLists.FromSqlRaw(queryList);

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
                result.NIK = header.NIK;
                result.Periode = header.Periode;
                result.Kerja = header.Kerja;
                result.Libur = header.Libur;
                result.TotalKerja = $"{header.Kerja} dari {hariKerjaTanpaLibur} hari kerja";
                result.vReportAbsensiLists = detail != null ? detail.ToList() : null;

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

        public ReportAbsensiPerTahunResponse getReportAbsensiPerTahun(int tahun)
        {
            var context = new EFContext();
            try
            {
                // Check Attendance per User                
                var users = context.Users.Where(x => x.IsDeleted != true && x.IsAdmin != true);
                foreach (var user in users)
                    CheckAttendanceAsync(user.UserID);

                // Get report function
                var libur = context.Calendars.Where(x => x.IsDeleted != true && x.Holiday.Year == tahun && x.Holiday.Year == tahun)
                                .ToList()
                                .Where(x => x.Holiday.DayOfWeek != DayOfWeek.Saturday && x.Holiday.DayOfWeek != DayOfWeek.Sunday)
                                .Count();

                var queryList = String.Format($@"
                    SELECT *
                    FROM
                        vReportAbsensiListPerTahun");

                var detail = context.vReportAbsensiListPerTahuns.FromSqlRaw(queryList).ToList();

                foreach (var data in detail)
                {
                    var checkAttendance = context.Attendances.Where(x => x.UserID == data.UserID && x.IsDeleted != true && x.Date.Value.Year == tahun);
                    if (checkAttendance != null)
                    {
                        data.Ontime = checkAttendance.Where(x => x.Status == "Ontime").Count();
                        data.WFH = checkAttendance.Where(x => x.Status == "WFH").Count();
                        data.Terlambat = checkAttendance.Where(x => x.Status == "Terlambat").Count();
                        data.Absen = checkAttendance.Where(x => x.Status == "Absen").Count();
                        data.Cuti = checkAttendance.Where(x => x.Status == "Cuti").Count();
                    }
                }

                int hariKerja = 0;

                for (int bulan = 1; bulan <= 12; bulan++)
                {
                    int daysInMonth = DateTime.DaysInMonth(tahun, bulan);

                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        DateTime currentDate = new DateTime(tahun, bulan, day);

                        if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                            hariKerja++;
                    }
                }

                int hariKerjaTanpaLibur = hariKerja - libur;

                return new ReportAbsensiPerTahunResponse(tahun.ToString(), libur, $"{hariKerjaTanpaLibur} hari kerja", detail != null ? detail : null);
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

        public ReportCutiResponse getReportCuti(Int64 userID, int bulan, int tahun)
        {
            var context = new EFContext();
            try
            {
                string namaBulan = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(bulan);
                var query = String.Format($@"
                    SELECT *
                    FROM
                        vReportCuti
                    WHERE
                        UserID = {userID} AND Periode = '{namaBulan} {tahun}'");

                var header = context.vReportCutis.FromSqlRaw(query).FirstOrDefault();
                if (header == null)
                    return null;

                var queryList = String.Format($@"
                    SELECT *
                    FROM
                        vReportCutiList
                    WHERE
                        UserID = {userID} AND Periode = '{namaBulan} {tahun}'");

                var detail = context.vReportCutiLists.FromSqlRaw(queryList).ToList();

                var querySisaCuti = String.Format($@"
                    SELECT *
                    FROM
                        vReportCutiPerTahun");

                var sisaCuti = context.vReportCutiPerTahuns.FromSqlRaw(querySisaCuti).FirstOrDefault();
                var user = context.Users.FirstOrDefault(x => x.UserID == userID && x.IsDeleted != true);
                var checkCuti = context.Attendances.Where(x => x.UserID == userID && x.IsDeleted != true && x.Status == "Cuti" && x.Date.Value.Year == tahun).Count();

                if (sisaCuti != null)
                    sisaCuti.SisaCuti = header.JatahCuti - checkCuti;

                return new ReportCutiResponse(header.Periode, header.UserID, header.Nama, header.Posisi, header.NIK, header.Cuti, header.JatahCuti, sisaCuti != null ? sisaCuti.SisaCuti : 0, detail != null ? detail : null);
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

        public ReportCutiPerTahunResponse getReportCutiPerTahun(int tahun)
        {
            var context = new EFContext();
            try
            {
                var query = String.Format($@"
                    SELECT *
                    FROM vReportCutiPerTahun");

                var detail = context.vReportCutiPerTahuns.FromSqlRaw(query).ToList();
                
                foreach (var data in detail)
                {
                    var user = context.Users.FirstOrDefault(x => x.UserID == data.UserID && x.IsDeleted != true);
                    var checkCuti = context.Attendances.Where(x => x.Status == "Cuti" && x.IsDeleted != true && x.UserID == user.UserID && x.Date.Value.Year == tahun).Count();

                    data.Cuti = checkCuti;
                    data.SisaCuti = data.JatahCuti - data.Cuti;
                }

                return new ReportCutiPerTahunResponse(tahun.ToString(), detail != null ? detail : null);
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

        public async Task CheckAttendanceAsync(Int64 userID)
        {
            using (var context = new EFContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.UserID == userID && x.IsDeleted != true);
                if (user == null)
                    return;

                var startDate = user.StartWork?.Date ?? throw new Exception("Tanggal mulai kerja belum diatur!");
                var endDate = DateTime.Now.AddHours(7).Date;

                // Retrieve holidays and existing attendances in bulk
                var holidays = await context.Calendars.Where(x => x.Holiday >= startDate && x.Holiday < endDate && x.IsDeleted != true).ToListAsync();
                var existingAttendances = await context.Attendances.Where(x => x.UserID == userID && x.Date >= startDate && x.Date < endDate && x.IsDeleted != true).ToListAsync();

                var newAttendances = new List<Attendance>();

                for (var currentDate = startDate; currentDate < endDate; currentDate = currentDate.AddDays(1))
                {
                    if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    var isHoliday = holidays.Any(x => x.Holiday.Date == currentDate);
                    if (isHoliday)
                        continue;

                    var existingAttendance = existingAttendances.FirstOrDefault(x => x.Date.Value.Date == currentDate);
                    if (existingAttendance == null)
                    {
                        newAttendances.Add(new Attendance
                        {
                            UserID = userID,
                            Date = currentDate,
                            ClockIn = currentDate,
                            ClockOut = currentDate,
                            Description = "",
                            Status = "Absen",
                            DateIn = DateTime.Now.AddHours(7),
                            UserIn = user.UserID.ToString(),
                            IsDeleted = false
                        });
                    }
                    else if (existingAttendance.ClockOut == null)
                    {
                        existingAttendance.Status = "Absen";
                        existingAttendance.ClockOut = currentDate;
                    }
                }

                if (newAttendances.Any())
                {
                    await context.Attendances.AddRangeAsync(newAttendances);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
