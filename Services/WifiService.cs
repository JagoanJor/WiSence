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
    public interface IWifiService<T> : IService<T>
    {
        T Create(T data, User user);
    }
    public class WifiService : IWifiService<Wifi>
    {
        public Wifi Create(Wifi data)
        {
            var context = new EFContext();
            try
            {
                /*int flag = 0;
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
                                    data.Name = wlanInterface.CurrentConnection.profileName;
                                }
                            }

                            data.IPAddress = gatewayInfo.Address.ToString();
                            flag = 1;
                            break;
                        }
                }

                if (flag == 0)
                    throw new Exception("Please Connect to Wi-fi!");

                context.Wifis.Add(data);
                context.SaveChanges();*/

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

        public Wifi Create(Wifi data, User user)
        {
            var context = new EFContext();
            try
            {
                int flag = 0;
                NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface networkInterface in networkInterfaces)
                {
                    // Log network interface details
                    Console.WriteLine($"Interface: {networkInterface.Name}, Type: {networkInterface.NetworkInterfaceType}");
                }

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
                                    data.Name = wlanInterface.CurrentConnection.profileName;
                                }
                            }

                            data.IPAddress = gatewayInfo.Address.ToString();
                            flag = 1;
                            break;
                        }
                }

                if (flag == 0)
                    throw new Exception("Please Connect to Wi-fi!");

                var pos = context.Positions.FirstOrDefault(x => x.PositionID == user.PositionID && x.IsDeleted != true);
                if (pos == null)
                    throw new Exception("Please set user's position!");

                var div = context.Divisions.FirstOrDefault(x => x.DivisionID == pos.DivisionID && x.IsDeleted != true);
                if (div == null)
                    throw new Exception("Please set position's division!");

                var com = context.Companies.FirstOrDefault(x => x.CompanyID == div.CompanyID && x.IsDeleted != true);
                if (com == null)
                    throw new Exception("Please set division's company!");

                data.CompanyID = com.CompanyID;

                context.Wifis.Add(data);
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
                var obj = context.Wifis.FirstOrDefault(x => x.WifiID == id && x.IsDeleted != true);
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

        public Wifi Edit(Wifi data)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Wifis.FirstOrDefault(x => x.WifiID == data.WifiID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.Name = data.Name;
                obj.IPAddress = data.IPAddress;
                obj.CompanyID = data.CompanyID;
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

        public IEnumerable<Wifi> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Wifis where a.IsDeleted != true select a;
                query = query.Include("Company");

                // Searching
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(x => x.Name.Contains(search)
                        || x.IPAddress.Contains(search));

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
                                case "name": query = query.Where(x => x.Name.Contains(value)); break;
                                case "ipaddress": query = query.Where(x => x.IPAddress.Contains(value)); break;
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
                            case "name": query = query.OrderByDescending(x => x.Name); break;
                            case "ipaddress": query = query.OrderByDescending(x => x.IPAddress); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "name": query = query.OrderBy(x => x.Name); break;
                            case "ipaddress": query = query.OrderBy(x => x.IPAddress); break;
                        }
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.WifiID);
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

        public Wifi GetById(Int64 id)
        {
            var context = new EFContext();
            try
            {
                return context.Wifis
                    .Include(x => x.Company)
                    .FirstOrDefault(x => x.WifiID == id && x.IsDeleted != true);
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

