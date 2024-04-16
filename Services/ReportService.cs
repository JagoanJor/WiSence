using API.Entities;
using API.Helpers;
using API.Responses;
using Microsoft.EntityFrameworkCore;
using NativeWifi;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace API.Services
{
    public interface IReportService
    {
        vReportAbsensi getReportAbsensi(Int64 userID, int bulan, int tahun);
        vReportAbsensiPerTahun getReportAbsensiPerTahun(int tahun);
        vReportCuti getReportCuti(Int64 userID, int bulan, int tahun);
        CutiReportResponse getReportCutiPerTahun(int tahun);
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
                        UserID = {userID} AND Periode = '{namaBulan} {tahun}'");

                var header = context.vReportAbsensis.FromSqlRaw(query).FirstOrDefault();
                if (header == null)
                    return null;

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

        public vReportAbsensiPerTahun getReportAbsensiPerTahun(int tahun)
        {
            var context = new EFContext();
            try
            {
                var result = new vReportAbsensiPerTahun();
                var query = String.Format($@"
                    SELECT *
                    FROM
                        vReportAbsensiPerTahun
                    WHERE
                        Periode = '{tahun}'");

                var header = context.vReportAbsensiPerTahuns.FromSqlRaw(query).FirstOrDefault();
                if (header == null)
                    return null;

                var queryList = String.Format($@"
                    SELECT *
                    FROM
                        vReportAbsensiListPerTahun
                    WHERE
                        Periode = '{tahun}'");

                var detail = context.vReportAbsensiListPerTahuns.FromSqlRaw(queryList);

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

                int hariKerjaTanpaLibur = hariKerja - header.Libur;

                result.Periode = header.Periode;
                result.Libur = header.Libur;
                result.TotalKerja = $"{hariKerjaTanpaLibur} hari kerja";
                result.vReportAbsensiListPerTahuns = detail != null ? detail.ToList() : null;

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

        public vReportCuti getReportCuti(Int64 userID, int bulan, int tahun)
        {
            var context = new EFContext();
            try
            {
                var result = new vReportCuti();
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

                var detail = context.vReportCutiLists.FromSqlRaw(queryList);

                result.Nama = header.Nama;
                result.UserID = userID;
                result.Posisi = header.Posisi;
                result.Periode = header.Periode;
                result.vReportCutiLists = detail != null ? detail.ToList() : null;

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

        public CutiReportResponse getReportCutiPerTahun(int tahun)
        {
            var context = new EFContext();
            try
            {
                var query = String.Format($@"
                    SELECT *
                    FROM
                        vReportCutiPerTahun
                    WHERE
                        Periode = '{tahun}'");

                var header = context.vReportCutiPerTahuns.FromSqlRaw(query).FirstOrDefault();
                if (header == null)
                    return null;

                var detail = context.vReportCutiPerTahuns.FromSqlRaw(query);

                return new CutiReportResponse(header.Periode, detail != null ? detail.ToList() : null);
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
