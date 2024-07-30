using API.Entities;
using API.Helpers;
using API.Responses;
using Microsoft.EntityFrameworkCore;
using MimeKit;
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
        Task<vReportAbsensi> getReportAbsensiAsync(Int64 userID, int bulan, int tahun);
        Task<ReportAbsensiPerTahunResponse> getReportAbsensiPerTahunAsync(int tahun);
        Task<ReportCutiResponse> getReportCutiAsync(Int64 userID, int bulan, int tahun);
        Task<ReportCutiPerTahunResponse> getReportCutiPerTahunAsync(int tahun);
    }

    public class ReportService : IReportService
    {
        public async Task<vReportAbsensi> getReportAbsensiAsync(Int64 userID, int bulan, int tahun)
        {
            using (var context = new EFContext())
            {
                try
                {
                    // Check user's attendance
                    CheckAttendanceAsync(userID);

                    // Get report function
                    var result = new vReportAbsensi();
                    string namaBulan = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(bulan);
                    var query = $@"
                    SELECT *
                    FROM
                        vReportAbsensi
                    WHERE
                        UserID = {userID}";

                    var header = await context.vReportAbsensis.FromSqlRaw(query).FirstOrDefaultAsync();
                    if (header == null)
                        return null;

                    header.Kerja = await context.Attendances.Where(x => x.UserID == header.UserID && x.IsDeleted != true && x.Date.Value.Month == bulan && x.Date.Value.Year == tahun && x.Status != "Cuti" && x.Status != "Absen").CountAsync();
                    header.Libur = context.Calendars.Where(x => x.IsDeleted != true && x.Holiday.Month == bulan && x.Holiday.Year == tahun)
                                .ToList()
                                .Where(x => x.Holiday.DayOfWeek != DayOfWeek.Saturday && x.Holiday.DayOfWeek != DayOfWeek.Sunday)
                                .Count();

                    header.Periode = $"{bulan} {tahun}";

                    var queryList = $@"
                    SELECT *
                    FROM
                        vReportAbsensiList
                    WHERE
                        UserID = {userID} AND Periode = '{namaBulan} {tahun}'";

                    var detail = await context.vReportAbsensiLists.FromSqlRaw(queryList).ToListAsync();

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
                    result.vReportAbsensiLists = detail;

                    return result;
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

        public async Task<ReportAbsensiPerTahunResponse> getReportAbsensiPerTahunAsync(int tahun)
        {
            using (var context = new EFContext())
            {
                try
                {
                    // Check Attendance per User                
                    var users = await context.Users.Where(x => x.IsDeleted != true && x.IsAdmin != true).ToListAsync();
                    foreach (var user in users)
                        CheckAttendanceAsync(user.UserID);

                    // Get report function
                    var libur = context.Calendars.Where(x => x.IsDeleted != true && x.Holiday.Year == tahun)
                                .ToList()
                                .Where(x => x.Holiday.DayOfWeek != DayOfWeek.Saturday && x.Holiday.DayOfWeek != DayOfWeek.Sunday)
                                .Count();

                    var queryList = @"
                    SELECT *
                    FROM
                        vReportAbsensiListPerTahun";

                    var detail = await context.vReportAbsensiListPerTahuns.FromSqlRaw(queryList).ToListAsync();

                    foreach (var data in detail)
                    {
                        var checkAttendance = await context.Attendances.Where(x => x.UserID == data.UserID && x.IsDeleted != true && x.Date.Value.Year == tahun).ToListAsync();
                        if (checkAttendance != null)
                        {
                            data.Ontime = checkAttendance.Count(x => x.Status == "Ontime");
                            data.WFH = checkAttendance.Count(x => x.Status == "WFH");
                            data.Terlambat = checkAttendance.Count(x => x.Status == "Terlambat");
                            data.Absen = checkAttendance.Count(x => x.Status == "Absen");
                            data.Cuti = checkAttendance.Count(x => x.Status == "Cuti");
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

                    return new ReportAbsensiPerTahunResponse(tahun.ToString(), libur, $"{hariKerjaTanpaLibur} hari kerja", detail);
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

        public async Task<ReportCutiResponse> getReportCutiAsync(Int64 userID, int bulan, int tahun)
        {
            using (var context = new EFContext())
            {
                try
                {
                    string namaBulan = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(bulan);
                    var query = $@"
                    SELECT *
                    FROM
                        vReportCuti
                    WHERE
                        UserID = {userID} AND Periode = '{namaBulan} {tahun}'";

                    var header = await context.vReportCutis.FromSqlRaw(query).FirstOrDefaultAsync();
                    if (header == null)
                        return null;

                    var queryList = $@"
                    SELECT *
                    FROM
                        vReportCutiList
                    WHERE
                        UserID = {userID} AND Periode = '{namaBulan} {tahun}'";

                    var detail = await context.vReportCutiLists.FromSqlRaw(queryList).ToListAsync();

                    var querySisaCuti = @"
                    SELECT *
                    FROM
                        vReportCutiPerTahun";

                    var sisaCuti = await context.vReportCutiPerTahuns.FromSqlRaw(querySisaCuti).FirstOrDefaultAsync();
                    var user = await context.Users.FirstOrDefaultAsync(x => x.UserID == userID && x.IsDeleted != true);
                    var checkCuti = await context.Attendances.CountAsync(x => x.UserID == userID && x.IsDeleted != true && x.Status == "Cuti" && x.Date.Value.Year == tahun);

                    if (sisaCuti != null)
                    {
                        sisaCuti.SisaCuti = header.JatahCuti - checkCuti;
                        if (sisaCuti.SisaCuti < 0)
                            sisaCuti.SisaCuti = 0;
                    }

                    return new ReportCutiResponse(header.Periode, header.UserID, header.Nama, header.Posisi, header.NIK, header.Cuti, header.JatahCuti, sisaCuti != null ? sisaCuti.SisaCuti : 0, detail);
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

        public async Task<ReportCutiPerTahunResponse> getReportCutiPerTahunAsync(int tahun)
        {
            using (var context = new EFContext())
            {
                try
                {
                    var query = @"
                    SELECT *
                    FROM vReportCutiPerTahun";

                    var detail = await context.vReportCutiPerTahuns.FromSqlRaw(query).ToListAsync();

                    foreach (var data in detail)
                    {
                        var user = await context.Users.FirstOrDefaultAsync(x => x.UserID == data.UserID && x.IsDeleted != true);
                        var checkCutiBefore = await context.Attendances
                            .CountAsync(x => x.UserID == user.UserID &&
                                x.IsDeleted != true &&
                                x.Status == "Cuti" &&
                                x.Date.Value.Month >= user.StartWork.Value.Month &&
                                x.Date.Value.Year == DateTime.Now.AddHours(7).Year);

                        var checkCutiAfter = await context.Attendances
                            .CountAsync(x => x.UserID == user.UserID &&
                                x.IsDeleted != true &&
                                x.Status == "Cuti" &&
                                x.Date.Value.Month < user.StartWork.Value.Month &&
                                x.Date.Value.Year == DateTime.Now.AddHours(7).AddYears(1).Year);

                        data.Cuti = checkCutiAfter + checkCutiBefore;
                        data.SisaCuti = data.JatahCuti - data.Cuti;
                        if (data.SisaCuti < 0)
                            data.SisaCuti = 0;
                    }

                    return new ReportCutiPerTahunResponse(tahun.ToString(), detail);
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

        public async Task CheckAttendanceAsync(Int64 userID)
        {
            using (var context = new EFContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.UserID == userID && x.IsDeleted != true);
                if (user == null)
                    return;

                var shift = await context.Shifts.FirstOrDefaultAsync(x => x.ShiftID == user.ShiftID && x.IsDeleted != true);
                if (shift == null)
                    throw new Exception($"Silahkan mengatur shift kerja {user.Name}!");

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
                            Shift = $"{shift.Description} ({shift.ClockIn} - {shift.ClockOut})",
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
