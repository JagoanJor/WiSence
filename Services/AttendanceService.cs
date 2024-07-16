using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Helpers;
using API.Responses;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public interface IAttendanceService
    {
        Task<ListResponse<Attendance>> GetAllAsync(int limit, int page, int total, string search, string sort, string filter, string date, User user);
        Task<Attendance> GetByIdAsync(long id);
        Task<Attendance> CreateAsync(Attendance data);
        Task<Attendance> EditAsync(Attendance data);
        Task<bool> DeleteAsync(long id, string userID);
        Task<Attendance> ClockInAsync(User user, double longitude, double latitude);
        Task<Attendance> ClockOutAsync(User user, double longitude, double latitude);
    }

    public class AttendanceService : IAttendanceService
    {
        public async Task<Attendance> CreateAsync(Attendance data)
        {
            using var context = new EFContext();
            try
            {
                context.Attendances.Add(data);
                await context.SaveChangesAsync();

                return data;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<bool> DeleteAsync(long id, string userID)
        {
            using var context = new EFContext();
            try
            {
                var obj = await context.Attendances.FirstOrDefaultAsync(x => x.AttendanceID == id && x.IsDeleted != true);
                if (obj == null) return false;

                obj.IsDeleted = true;
                obj.UserUp = userID;
                obj.DateUp = DateTime.Now.AddHours(7);

                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<Attendance> EditAsync(Attendance data)
        {
            using var context = new EFContext();
            try
            {
                var obj = await context.Attendances.FirstOrDefaultAsync(x => x.AttendanceID == data.AttendanceID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.UserID = data.UserID;
                obj.Status = data.Status;
                obj.Description = data.Description;
                obj.Date = data.Date;
                obj.ClockIn = data.ClockIn;
                obj.ClockOut = data.ClockOut;
                obj.UserUp = data.UserUp;
                obj.DateUp = DateTime.Now.AddHours(7);

                await context.SaveChangesAsync();

                return obj;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<ListResponse<Attendance>> GetAllAsync(int limit, int page, int total, string search, string sort, string filter, string date, User user)
        {
            using var context = new EFContext();
            try
            {
                var query = context.Attendances.Where(a => a.IsDeleted != true).Include("User");

                // If not Admin, just return the user data
                if (user.IsAdmin != true)
                    query = query.Where(x => x.UserID == user.UserID);

                // Check user's attendance
                foreach (var users in query)
                    CheckAttendanceAsync(users.UserID);

                // Date
                if (!string.IsNullOrEmpty(date))
                {
                    var dateList = date.Split("|", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var d in dateList)
                    {
                        var dates = d.Split(":", StringSplitOptions.RemoveEmptyEntries);
                        if (dates.Length == 2)
                        {
                            var fieldName = dates[0].Trim().ToLower();
                            if (fieldName == "startdate")
                            {
                                DateTime.TryParse(dates[1].Trim(), out DateTime startDate);
                                query = query.Where(x => x.Date >= startDate);
                            }
                            else if (fieldName == "enddate")
                            {
                                DateTime.TryParse(dates[1].Trim(), out DateTime endDate);
                                endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);
                                query = query.Where(x => x.Date <= endDate);
                            }
                        }
                    }
                }

                // Searching
                if (!string.IsNullOrEmpty(search))
                {
                    if (DateTime.TryParse(search, out DateTime searchDate))
                        query = query.Where(x => x.Date == searchDate.Date
                        || (x.Date.Value.Month == searchDate.Month && x.Date.Value.Day == searchDate.Day));
                    else
                        query = query.Where(x => x.Description.Contains(search)
                        || x.Status.Contains(search)
                        || x.User.Name.Contains(search)
                        || x.User.NIK.Contains(search)
                        || x.ClockIn.ToString().Contains(search)
                        || x.ClockOut.ToString().Contains(search)
                        || x.Date.ToString().Contains(search));
                }

                // Filtering
                if (!string.IsNullOrEmpty(filter))
                {
                    var filterList = filter.Split("|", StringSplitOptions.RemoveEmptyEntries);
                    foreach (var f in filterList)
                    {
                        var searchList = f.Split(":", StringSplitOptions.RemoveEmptyEntries);
                        if (searchList.Length == 2)
                        {
                            var fieldName = searchList[0].Trim().ToLower();
                            var value = searchList[1].Trim();
                            switch (fieldName)
                            {
                                case "status": query = query.Where(x => x.Status.Contains(value)); break;
                                case "description": query = query.Where(x => x.Description.Contains(value)); break;
                                case "userid": query = query.Where(x => x.User.UserID.ToString().Contains(value)); break;
                                case "name": query = query.Where(x => x.User.Name.Contains(value)); break;
                                case "nik": query = query.Where(x => x.User.NIK.Contains(value)); break;
                                case "clockin":
                                    DateTime.TryParse(value, out DateTime searchClockIn);
                                    query = query.Where(x => x.ClockIn.Value.Date == searchClockIn.Date || x.ClockIn.Value.Hour == searchClockIn.Hour || x.ClockIn.Value.Minute == searchClockIn.Minute);
                                    break;
                                case "clockout":
                                    DateTime.TryParse(value, out DateTime searchClockOut);
                                    query = query.Where(x => x.ClockOut.Value.Date == searchClockOut.Date || x.ClockOut.Value.Hour == searchClockOut.Hour || x.ClockOut.Value.Minute == searchClockOut.Minute);
                                    break;
                                case "date":
                                    var searchDate = value.Split("/", StringSplitOptions.RemoveEmptyEntries);
                                    DateTime.TryParse(searchDate[0].Trim(), out DateTime searchStart);
                                    DateTime.TryParse(searchDate[1].Trim(), out DateTime searchEnd);
                                    query = query.Where(x => x.Date.Value.Date >= searchStart.Date && x.Date.Value.Date <= searchEnd.Date);
                                    break;
                            }
                        }
                    }
                }

                // Sorting
                if (!string.IsNullOrEmpty(sort))
                {
                    var temp = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var orderBy = sort;
                    if (temp.Length > 1)
                        orderBy = temp[0];

                    if (temp.Length > 1)
                    {
                        switch (orderBy.ToLower())
                        {
                            case "userid": query = query.OrderByDescending(x => x.User.UserID); break;
                            case "name": query = query.OrderByDescending(x => x.User.Name); break;
                            case "status": query = query.OrderByDescending(x => x.Status); break;
                            case "description": query = query.OrderByDescending(x => x.Description); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "userid": query = query.OrderBy(x => x.User.UserID); break;
                            case "name": query = query.OrderBy(x => x.User.Name); break;
                            case "status": query = query.OrderBy(x => x.Status); break;
                            case "description": query = query.OrderBy(x => x.Description); break;
                        }
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.Date);
                }

                // Get Total Before Limit and Page
                total = await query.CountAsync();

                // Set Limit and Page
                if (limit != 0)
                    query = query.Skip(page * limit).Take(limit);

                // Get Data
                var data = await query.ToListAsync();
                if (data.Count <= 0 && page > 0)
                {
                    page = 0;
                    return await GetAllAsync(limit, page, total, search, sort, filter, date, user);
                }

                return new ListResponse<Attendance>(data, total, page); ;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<Attendance> GetByIdAsync(long id)
        {
            using var context = new EFContext();
            try
            {
                // Check user's attendance
                var attendance = await context.Attendances.FirstOrDefaultAsync(x => x.AttendanceID == id && x.IsDeleted != true);
                CheckAttendanceAsync((long)attendance.UserID);

                return attendance;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<Attendance> ClockInAsync(User user, double longitude, double latitude)
        {
            using var context = new EFContext();
            try
            {
                // Check attendance today
                var attendance = await context.Attendances.FirstOrDefaultAsync(x => x.UserID == user.UserID && x.IsDeleted != true && x.Date.Value.Date == DateTime.Now.AddHours(7).Date && (x.Status == "Cuti" || x.Status == "WFH"));
                if (attendance != null)
                    throw new Exception($"User sudah memiliki data absensi dengan status {attendance.Status}!");

                var company = await context.Companies.FirstOrDefaultAsync(x => x.CompanyID == user.CompanyID && x.IsDeleted != true);
                if (company.MaxRange == null)
                    throw new Exception("Silahkan menghubungi admin untuk mengatur jarak maksimal absensi!");

                var checkLoc = await context.Locations.FirstOrDefaultAsync(x => x.CompanyID == user.CompanyID && x.IsDeleted != true);
                if (checkLoc == null)
                    throw new Exception("Belum ada data Lokasi!");

                var location = await context.Locations.Where(x => x.CompanyID == user.CompanyID && x.IsDeleted != true).ToListAsync();
                var range = 0.0;
                var index = 0;
                var lastIndex = location.Count - 1;
                var nearestLocation = "";
                var nearestRange = 0.0;

                foreach (var loc in location)
                {
                    range = GetDistance(longitude, latitude, loc.Longtitude, loc.Latitude);
                    if (nearestRange == 0 || nearestRange > range)
                    {
                        nearestRange = range;
                        nearestLocation = loc.Name;
                    }

                    if (range <= company.MaxRange)
                        break;
                    if (range > company.MaxRange && index == lastIndex)
                        throw new Exception($"Jarak anda saat ini adalah {Math.Round(range, 2)} km! Clock In hanya bisa dilakukan dalam jarak {company.MaxRange} km dari Kantor! Lokasi terdekat adalah {nearestLocation}.");
                    index++;
                }

                var shift = await context.Shifts.FirstOrDefaultAsync(x => x.ShiftID == user.ShiftID && x.IsDeleted != true);
                if (shift == null)
                    throw new Exception("Silahkan menghubungi admin untuk mengatur shift kerja anda!");

                if (DateTime.Now.AddHours(7).TimeOfDay < shift.ClockIn.Value.AddMinutes(-30).TimeOfDay)
                    throw new Exception($"Clock In hanya bisa dilakukan 30 menit sebelum pukul {shift.ClockIn?.ToString("HH:mm")}.");

                var data = new Attendance();

                data.Description = "";
                data.UserID = user.UserID;
                data.ClockIn = DateTime.Now.AddHours(7);
                data.UserIn = user.UserID.ToString();
                data.DateIn = DateTime.Now.AddHours(7);
                data.Date = DateTime.Now.AddHours(7);
                data.IsDeleted = false;

                DateTime lateTime = shift.ClockIn.Value.AddHours(1).AddMinutes(30);

                if (DateTime.Now.AddHours(7).TimeOfDay <= shift.ClockIn?.TimeOfDay)
                    data.Status = "Ontime";
                else if (DateTime.Now.AddHours(7).TimeOfDay >= shift.ClockIn?.TimeOfDay && DateTime.Now.AddHours(7).TimeOfDay < lateTime.TimeOfDay)
                    data.Status = "Terlambat";
                else
                    data.Status = "Absen";

                // Check user's attendance
                CheckAttendanceAsync(user.UserID);

                context.Attendances.Add(data);
                await context.SaveChangesAsync();

                return data;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<Attendance> ClockOutAsync(User user, double longitude, double latitude)
        {
            using var context = new EFContext();
            try
            {
                var data = await context.Attendances
                    .Where(x => x.UserID == user.UserID && x.Date.Value.Date == DateTime.Now.AddHours(7).Date && x.IsDeleted != true)
                    .FirstOrDefaultAsync();

                if (data == null)
                    throw new Exception("Pastikan kamu sudah melakukan Clock In!");

                var company = await context.Companies.FirstOrDefaultAsync(x => x.CompanyID == user.CompanyID && x.IsDeleted != true);
                if (company.MaxRange == null)
                    throw new Exception("Silahkan menghubungi admin untuk mengatur jarak maksimal absensi!");

                var location = await context.Locations.Where(x => x.CompanyID == user.CompanyID && x.IsDeleted != true).ToListAsync();

                var range = 0.0;
                var index = 0;
                var lastIndex = location.Count - 1;

                foreach (var loc in location)
                {
                    range = GetDistance(longitude, latitude, loc.Longtitude, loc.Latitude);
                    if (range <= company.MaxRange)
                        break;
                    if (range > company.MaxRange && index == lastIndex)
                        throw new Exception($"Jarak anda saat ini adalah {range} km! Clock In hanya bisa dilakukan dalam jarak {company.MaxRange} km dari Kantor!");
                    index++;
                }

                var shift = await context.Shifts.FirstOrDefaultAsync(x => x.ShiftID == user.ShiftID && x.IsDeleted != true);
                if (shift == null)
                    throw new Exception("Hubungi admin untuk mengatur shift kerja anda!");

                if (DateTime.Now.AddHours(7).TimeOfDay < shift.ClockIn?.TimeOfDay)
                    throw new Exception($"Clock Out hanya dapat dilakukan saat dan setelah pukul {shift.ClockIn?.ToString("HH:mm")}.");

                data.ClockOut = DateTime.Now.AddHours(7);
                data.UserUp = user.UserID.ToString();
                data.DateUp = DateTime.Now.AddHours(7);

                await context.SaveChangesAsync();

                return data;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        // Function to make sure there is no empty attendance
        public async Task CheckAttendanceAsync(long? userID)
        {
            using var context = new EFContext();
            var user = await context.Users.FirstOrDefaultAsync(x => x.UserID == userID && x.IsDeleted != true);
            if (user == null)
                return;

            var currentDate = user.StartWork.Value.Date;
            if (currentDate == null)
                throw new Exception("Tanggal mulai kerja user belum diatur!");

            while (currentDate.Date < DateTime.Now.AddHours(7).Date)
            {
                var haveAttend = await context.Attendances.FirstOrDefaultAsync(x => x.Date.Value.Date == currentDate.Date && x.IsDeleted != true && x.UserID == userID);

                if (currentDate.DayOfWeek.ToString() != "Saturday" && currentDate.DayOfWeek.ToString() != "Sunday")
                {
                    var holiday = await context.Calendars.FirstOrDefaultAsync(x => x.Holiday.Date == currentDate.Date && x.IsDeleted != true);
                    if (holiday == null)
                    {
                        if (haveAttend == null)
                        {
                            var attendance = new Attendance();
                            attendance.UserID = userID;
                            attendance.Date = currentDate;
                            attendance.ClockIn = currentDate;
                            attendance.ClockOut = currentDate;
                            attendance.Description = "";
                            attendance.Status = "Absen";
                            attendance.DateIn = DateTime.Now.AddHours(7);
                            attendance.UserIn = (await context.Users.FirstOrDefaultAsync(x => x.UserID == userID && x.IsDeleted != true)).UserID.ToString();
                            attendance.IsDeleted = false;

                            context.Attendances.Add(attendance);
                        }
                        else
                        {
                            if (haveAttend.ClockOut == null)
                            {
                                haveAttend.Status = "Absen";
                                haveAttend.ClockOut = currentDate;

                                context.Attendances.Update(haveAttend);
                            }
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            await context.SaveChangesAsync();
        }

        public double GetDistance(double long1, double lat1, double long2, double lat2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(long2 - long1);

            var lati1 = ToRadians(lat1);
            var lati2 = ToRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lati1) * Math.Cos(lati2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return 6371.0 * c;
        }

        private static double ToRadians(double angle)
        {
            return Math.PI / 180 * angle;
        }
    }
}
