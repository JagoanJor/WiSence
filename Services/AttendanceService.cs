using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using API.Entities;
using API.Helpers;

using Microsoft.EntityFrameworkCore;
using NativeWifi;

namespace API.Services
{
    public interface IAttendanceService
    {
        IEnumerable<Attendance> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date, User user);
        Attendance GetById(Int64 id);
        Attendance Create(Attendance data);
        Attendance Edit(Attendance data);
        bool Delete(Int64 id, String userID);
        Attendance ClockIn(User user, Double longtitude, Double latitude);
        Attendance ClockOut(User user, Double longtitude, Double latitude);
    }
    public class AttendanceService : IAttendanceService
    {
        public Attendance Create(Attendance data)
        {
            var context = new EFContext();
            try
            {
                context.Attendances.Add(data);
                context.SaveChanges();

                return data;
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

        public bool Delete(Int64 id, String userID)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Attendances.FirstOrDefault(x => x.AttendanceID == id && x.IsDeleted != true);
                if (obj == null) return false;

                obj.IsDeleted = true;
                obj.UserUp = userID;
                obj.DateUp = DateTime.Now.AddHours(7);

                context.SaveChanges();

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

        public Attendance Edit(Attendance data)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Attendances.FirstOrDefault(x => x.AttendanceID == data.AttendanceID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.UserID = data.UserID;
                obj.Status = data.Status;
                obj.Description = data.Description;
                obj.Date = data.Date;
                obj.ClockIn = data.ClockIn;
                obj.ClockOut = data.ClockOut;
                obj.UserUp = data.UserUp;
                obj.DateUp = DateTime.Now.AddHours(7);

                context.SaveChanges();

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

        public IEnumerable<Attendance> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date, User user)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Attendances where a.IsDeleted != true select a;
                query = query.Include("User");

                // If not Admin, just return the user data
                if (user.IsAdmin != true)
                    query = query.Where(x => x.UserID == user.UserID);

                // Check user's attendance
                foreach (var users in query)
                    CheckAttendance(users.UserID);

                //Date
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
                total = query.Count();

                // Set Limit and Page
                if (limit != 0)
                    query = query.Skip(page * limit).Take(limit);

                // Get Data
                var data = query.ToList();
                if (data.Count <= 0 && page > 0)
                {
                    page = 0;
                    return GetAll(limit, ref page, ref total, search, sort, filter, date, user);
                }

                return data;
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

        public Attendance GetById(Int64 id)
        {
            var context = new EFContext();
            try
            {
                // Check user's attendance
                var attendance = context.Attendances.FirstOrDefault(x => x.AttendanceID == id && x.IsDeleted != true);
                CheckAttendance((Int64)(attendance.UserID));

                return attendance;
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

        /*public Attendance ClockIn(User user)
        {
            var context = new EFContext();
            try
            {
                // Check attendance today
                var attendance = context.Attendances.FirstOrDefault(x => x.UserID == user.UserID && x.IsDeleted != true && x.Date.Value.Date == DateTime.Now.AddHours(7).Date && (x.Status == "Cuti" || x.Status == "WFH"));
                if (attendance != null)
                    throw new Exception($"User sudah memiliki data absensi dengan status {attendance.Status}!");

                var wifiSSID = "";
                var ipAddress = "";
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    GatewayIPAddressInformation gatewayInfo = ipProperties.GatewayAddresses.FirstOrDefault();

                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                        if (gatewayInfo != null)
                        {
                            WlanClient client = new WlanClient();
                            foreach (WlanClient.WlanInterface wlanInterface in client.Interfaces)
                            {
                                if (wlanInterface.InterfaceGuid == new Guid(networkInterface.Id))
                                {
                                    wifiSSID = wlanInterface.CurrentConnection.profileName;
                                }
                            }

                            ipAddress = gatewayInfo.Address.ToString();
                            break;
                        }
                }

                var query = String.Format($@"
                                SELECT TOP 1 W.WifiID, W.Name, W.IPAddress, W.CompanyID, W.DateIn, W.UserIn, W.DateUp, W.UserUp, W.IsDeleted
                                FROM Wifi AS W
                                INNER JOIN Company AS C ON C.CompanyID = W.CompanyID
                                WHERE C.IsDeleted != 1 AND W.IsDeleted != 1 AND W.IPAddress = '{ipAddress}' AND W.Name = '{wifiSSID}' AND W.CompanyID = {user.CompanyID}");
                var wifi = context.Wifis.FromSqlRaw(query).FirstOrDefault();

                if (wifi == null)
                    throw new Exception("Perangkat belum terhubung ke Wifi!");
                
                var shift = context.Shifts.FirstOrDefault(x => x.ShiftID == user.ShiftID && x.IsDeleted != true);
                if (shift == null)
                    throw new Exception("Silahkan menghubungi admin untuk mengatur shift kerja anda!");

                if (DateTime.Now.AddHours(7).TimeOfDay < shift.ClockIn.Value.AddMinutes(-30).TimeOfDay)
                    throw new Exception($"Clock In hanya bisa dilakukan 30 menit sebelum pukul {shift.ClockIn?.ToString("HH:mm")}.");

                var data = new Attendance();

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
                CheckAttendance(user.UserID);
                
                context.Attendances.Add(data);
                context.SaveChanges();

                return data;
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
        }*/

        public Attendance ClockIn(User user, Double longtitude, Double latitude)
        {
            var context = new EFContext();
            try
            {
                // Check attendance today
                var attendance = context.Attendances.FirstOrDefault(x => x.UserID == user.UserID && x.IsDeleted != true && x.Date.Value.Date == DateTime.Now.AddHours(7).Date && (x.Status == "Cuti" || x.Status == "WFH"));
                if (attendance != null)
                    throw new Exception($"User sudah memiliki data absensi dengan status {attendance.Status}!");

                var company = context.Companies.FirstOrDefault(x => x.CompanyID == user.CompanyID && x.IsDeleted != true);
                if (company.MaxRange == null)
                    throw new Exception("Silahkan menghubungi admin untuk mengatur jarak maksimal absensi!");

                var location = context.Locations.Where(x => x.CompanyID == user.CompanyID && x.IsDeleted != true);

                var range = 0.0;
                var index = 0;
                var lastIndex = location.Count() - 1;

                foreach (var loc in location)
                {
                    range = GetDistance(longtitude, latitude, loc.Longtitude, loc.Latitude);
                    if (range <= company.MaxRange)
                        break;
                    if (range > company.MaxRange && index == lastIndex)
                        throw new Exception($"Jarak anda saat ini adalah {Math.Round(range, 2)} km! Clock In hanya bisa dilakukan dalam jarak {company.MaxRange} km dari Kantor!");
                    index++;
                }

                var shift = context.Shifts.FirstOrDefault(x => x.ShiftID == user.ShiftID && x.IsDeleted != true);
                if (shift == null)
                    throw new Exception("Silahkan menghubungi admin untuk mengatur shift kerja anda!");

                if (DateTime.Now.AddHours(7).TimeOfDay < shift.ClockIn.Value.AddMinutes(-30).TimeOfDay)
                    throw new Exception($"Clock In hanya bisa dilakukan 30 menit sebelum pukul {shift.ClockIn?.ToString("HH:mm")}.");

                var data = new Attendance();

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
                CheckAttendance(user.UserID);
                
                context.Attendances.Add(data);
                context.SaveChanges();

                return data;
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

        /*public Attendance ClockOut(User user)
        {
            var context = new EFContext();
            try
            {
                var wifiSSID = "";
                var ipAddress = "";
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    GatewayIPAddressInformation gatewayInfo = ipProperties.GatewayAddresses.FirstOrDefault();

                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                        if (gatewayInfo != null)
                        {
                            WlanClient client = new WlanClient();
                            foreach (WlanClient.WlanInterface wlanInterface in client.Interfaces)
                            {
                                if (wlanInterface.InterfaceGuid == new Guid(networkInterface.Id))
                                {
                                    wifiSSID = wlanInterface.CurrentConnection.profileName;
                                }
                            }

                            ipAddress = gatewayInfo.Address.ToString();
                            break;
                        }
                }

                var query = String.Format($@"
                                SELECT TOP 1 WifiID, Name, IPAddress, CompanyID, DateIn, UserIn, DateUp, UserUp, IsDeleted
                                FROM Wifi
                                WHERE (IsDeleted = 0 OR IsDeleted IS NULL) AND (IPAddress = '{ipAddress}' OR Name = '{wifiSSID}')");
                var wifi = context.Wifis.FromSqlRaw(query).FirstOrDefault();

                if (wifi != null)
                {
                    if (wifi.Name != wifiSSID || wifi.IPAddress != ipAddress)
                        throw new Exception("Perangkat belum terhubung ke Wifi!");
                }
                else
                {
                    throw new Exception("Perangkat belum terhubung ke Wifi!");
                }

                var data = context.Attendances
                    .Where(x => x.UserID == user.UserID && x.Date.Value.Date == DateTime.Now.AddHours(7).Date && x.IsDeleted != true)
                    .FirstOrDefault();

                if (data == null)
                    throw new Exception("Pastikan kamu sudah melakukan Clock In!");

                var shift = context.Shifts.FirstOrDefault(x => x.ShiftID == user.ShiftID && x.IsDeleted != true);
                if (shift == null)
                    throw new Exception("Hubungi admin untuk mengatur shift kerja anda!");

                if (DateTime.Now.AddHours(7).TimeOfDay < shift.ClockIn?.TimeOfDay)
                    throw new Exception($"Clock Out hanya dapat dilakukan saat dan setelah pukul {shift.ClockIn?.ToString("HH:mm")}.");

                data.ClockOut = DateTime.Now.AddHours(7);
                data.UserUp = user.UserID.ToString();
                data.DateUp = DateTime.Now.AddHours(7);

                context.Attendances.Update(data);
                context.SaveChanges();

                return data;
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
        }*/
        
        public Attendance ClockOut(User user, Double longtitude, Double latitude)
        {
            var context = new EFContext();
            try
            {
                var data = context.Attendances
                    .Where(x => x.UserID == user.UserID && x.Date.Value.Date == DateTime.Now.AddHours(7).Date && x.IsDeleted != true)
                    .FirstOrDefault();

                if (data == null)
                    throw new Exception("Pastikan kamu sudah melakukan Clock In!");

                var company = context.Companies.FirstOrDefault(x => x.CompanyID == user.CompanyID && x.IsDeleted != true);
                if (company.MaxRange == null)
                    throw new Exception("Silahkan menghubungi admin untuk mengatur jarak maksimal absensi!");

                var location = context.Locations.Where(x => x.CompanyID == user.CompanyID && x.IsDeleted != true);

                var range = 0.0;
                var index = 0;
                var lastIndex = location.Count() - 1;

                foreach (var loc in location)
                {
                    range = GetDistance(longtitude, latitude, loc.Longtitude, loc.Latitude);
                    if (range <= company.MaxRange)
                        break;
                    if (range > company.MaxRange && index == lastIndex)
                        throw new Exception($"Jarak anda saat ini adalah {range} km! Clock In hanya bisa dilakukan dalam jarak {company.MaxRange} km dari Kantor!");
                    index++;
                }

                var shift = context.Shifts.FirstOrDefault(x => x.ShiftID == user.ShiftID && x.IsDeleted != true);
                if (shift == null)
                    throw new Exception("Hubungi admin untuk mengatur shift kerja anda!");

                if (DateTime.Now.AddHours(7).TimeOfDay < shift.ClockIn?.TimeOfDay)
                    throw new Exception($"Clock Out hanya dapat dilakukan saat dan setelah pukul {shift.ClockIn?.ToString("HH:mm")}.");

                data.ClockOut = DateTime.Now.AddHours(7);
                data.UserUp = user.UserID.ToString();
                data.DateUp = DateTime.Now.AddHours(7);

                context.Attendances.Update(data);
                context.SaveChanges();

                return data;
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

        // Function to make sure there is no empty attendance
        public void CheckAttendance(Int64? userID)
        {
            var context = new EFContext();
            var user = context.Users.FirstOrDefault(x => x.UserID == userID && x.IsDeleted != true);
            if (user == null)
                return;

            var currentDate = user.StartWork.Value.Date;
            if (currentDate == null)
                throw new Exception("Tanggal mulai kerja user belum diatur!");

            while (currentDate.Date < DateTime.Now.AddHours(7).Date)
            {
                var haveAttend = context.Attendances.FirstOrDefault(x => x.Date.Value.Date == currentDate.Date && x.IsDeleted != true && x.UserID == userID);

                if (currentDate.DayOfWeek.ToString() != "Saturday" && currentDate.DayOfWeek.ToString() != "Sunday")
                {
                    var holiday = context.Calendars.FirstOrDefault(x => x.Holiday.Date == currentDate.Date && x.IsDeleted != true);
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
                            attendance.UserIn = context.Users.FirstOrDefault(x => x.UserID == userID && x.IsDeleted != true).UserID.ToString();
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

            context.SaveChanges();
        }


        public double GetDistance(Double long1, Double lat1, Double long2, Double lat2)
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

        private static double ToRadians(Double angle)
        {
            return Math.PI / 180 * angle;
        }
    }
}