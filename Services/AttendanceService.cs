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
    public interface IAttendanceService<T> : IService<T>
    {
        T ClockIn(User user);
        T ClockOut(User user);
    }
    public class AttendanceService : IAttendanceService<Attendance>
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
                var obj = context.Attendances.FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
                if (obj == null) return false;

                obj.IsDeleted = true;
                obj.UserUp = userID;
                obj.DateUp = DateTime.Now.AddMinutes(-2);

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
                var obj = context.Attendances.FirstOrDefault(x => x.ID == data.ID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.UserID = data.UserID;
                obj.Status = data.Status;
                obj.Description = data.Description;
                obj.ClockIn = data.ClockIn;
                obj.ClockOut = data.ClockOut;
                obj.UserUp = data.UserUp;
                obj.DateUp = DateTime.Now.AddMinutes(-2);

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

        public IEnumerable<Attendance> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Attendances where a.IsDeleted != true select a;
                query = query.Include("User");

                // Searching
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(x => x.Description.Contains(search)
                        || x.Status.Contains(search)
                        || x.User.Name.Contains(search)
                        || x.ClockIn.ToString().Contains(search)
                        || x.ClockOut.ToString().Contains(search)
                        || x.Date.ToString().Contains(search));

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
                                case "name": query = query.Where(x => x.User.Name.Contains(value)); break;
                                case "clockin":
                                    DateTime.TryParse(value, out DateTime searchClockIn);
                                    query = query.Where(x => x.ClockIn == searchClockIn || x.ClockIn.Value.Hour == searchClockIn.Hour || x.ClockIn.Value.Minute == searchClockIn.Minute);
                                    break;
                                case "clockout":
                                    DateTime.TryParse(value, out DateTime searchClockOut);
                                    query = query.Where(x => x.ClockOut == searchClockOut || x.ClockOut.Value.Hour == searchClockOut.Hour || x.ClockOut.Value.Minute == searchClockOut.Minute);
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
                            case "name": query = query.OrderByDescending(x => x.User.Name); break;
                            case "status": query = query.OrderByDescending(x => x.Status); break;
                            case "description": query = query.OrderByDescending(x => x.Description); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "name": query = query.OrderBy(x => x.User.Name); break;
                            case "status": query = query.OrderBy(x => x.Status); break;
                            case "description": query = query.OrderBy(x => x.Description); break;
                        }
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.ID);
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
                    return GetAll(limit, ref page, ref total, search, sort, filter, date);
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
                return context.Attendances.FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
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

        public Attendance ClockIn(User user)
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
                                SELECT TOP 1 ID, Name, IPAddress, CompanyID, DateIn, UserIn, DateUp, UserUp, IsDeleted
                                FROM Wifi
                                WHERE (IsDeleted = 0 OR IsDeleted IS NULL) AND (IPAddress = '{ipAddress}' OR Name = '{wifiSSID}')");
                var wifi = context.Wifis.FromSqlRaw(query).FirstOrDefault();

                if (wifi != null)
                {
                    if (wifi.Name != wifiSSID || wifi.IPAddress != ipAddress)
                        throw new Exception("Please Connect to Company Wifi!");
                }
                else
                {
                    throw new Exception("Please Connect to Company Wifi!");
                }

                var WorkHour = context.Companies.FirstOrDefault();
                if (WorkHour == null)
                    throw new Exception("Please ask your admin to add the Working Hour data!");
                
                DateTime desiredTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, WorkHour.Start.Value.Hour, WorkHour.Start.Value.Minute, 0).AddMinutes(-30);
                
                if (DateTime.Now < desiredTime)
                    throw new Exception($"Clock In can only be done 30 minutes before the start of working hours {WorkHour.Start}.");

                var CheckAttendance = context.Attendances.FirstOrDefault(x => x.UserID == user.ID && DateTime.Now.Date == x.ClockIn.Value.Date);
                if (CheckAttendance != null)
                    throw new Exception("You already Clock In today. Press 'Re-Clock In' button first.");
                
                var data = new Attendance();

                data.UserID = user.ID;
                data.ClockIn = DateTime.Now;
                data.UserIn = user.ID.ToString();
                data.DateIn = DateTime.Now;
                data.Date = DateTime.Now;
                data.IsDeleted = false;

                if (DateTime.Now.TimeOfDay <= desiredTime.TimeOfDay)
                    data.Status = "Ontime";
                else if (DateTime.Now.TimeOfDay > desiredTime.TimeOfDay && DateTime.Now.TimeOfDay < desiredTime.AddHours(1).TimeOfDay)
                    data.Status = "Terlambat";
                else
                    data.Status = "Absen";

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

        public Attendance ClockOut(User user)
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
                                SELECT TOP 1 ID, Name, IPAddress, CompanyID, DateIn, UserIn, DateUp, UserUp, IsDeleted
                                FROM Wifi
                                WHERE (IsDeleted = 0 OR IsDeleted IS NULL) AND (IPAddress = '{ipAddress}' OR Name = '{wifiSSID}')");
                var wifi = context.Wifis.FromSqlRaw(query).FirstOrDefault();

                if (wifi != null)
                {
                    if (wifi.Name != wifiSSID || wifi.IPAddress != ipAddress)
                        throw new Exception("Please Connect to Company Wifi!");
                }
                else
                {
                    throw new Exception("Please Connect to Company Wifi!");
                }

                var data = context.Attendances
                    .Where(x => x.UserID == user.ID && x.Date.Value.Date == DateTime.Now.Date && x.IsDeleted != true)
                    .FirstOrDefault();

                if (data == null)
                    throw new Exception("Please ensure that you have clocked in.");

                data.ClockOut = DateTime.Now;
                data.UserUp = user.ID.ToString();
                data.DateUp = DateTime.Now;

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
    }
}

