﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using API.Entities;
using API.Helpers;
using API.Responses;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public interface ICutiService<T> : IService<T>
    {
        Company SetCuti(int id, int jatah, User user);
        int SisaCuti(Int64 userID, Int64 companyID);
    }
    public class CutiService : ICutiService<Cuti>
    {
        public Cuti Create(Cuti data)
        {
            var context = new EFContext();
            try
            {
                var currentDate = data.Start.Value.Date;

                //Validate to make sure there are no holiday or saturday or sunday set as Cuti
                for (int i = 1; i <= data.Durasi; i++)
                {
                    while(true)
                    {
                        var holiday = context.Calendars.FirstOrDefault(x => x.Holiday.Date == currentDate && x.IsDeleted != true);
                        if (holiday == null && currentDate.DayOfWeek.ToString() != "Saturday" && currentDate.DayOfWeek.ToString() != "Sunday")
                        {
                            if (i == data.Durasi)
                                data.End = currentDate;
                            break;
                        }

                        currentDate = currentDate.AddDays(1);
                    };

                    var attendance = new Attendance();
                    attendance.UserID = data.UserID;
                    attendance.Date = currentDate;
                    attendance.ClockIn = currentDate;
                    attendance.ClockOut = currentDate;
                    attendance.Description = data.Description;
                    attendance.Status = "Cuti";
                    attendance.DateIn = DateTime.Now;
                    attendance.UserIn = data.UserIn;
                    attendance.IsDeleted = false;

                    currentDate = currentDate.AddDays(1);

                    context.Attendances.Add(attendance);
                }

                context.Cutis.Add(data);
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
                var obj = context.Cutis.FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
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

        public Cuti Edit(Cuti data)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Cutis.FirstOrDefault(x => x.ID == data.ID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.Description = data.Description;
                obj.Start = data.Start;
                obj.End = data.End;
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

        public IEnumerable<Cuti> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Cutis where a.IsDeleted != true select a;
                query = query.Include("User");

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
                                query = query.Where(x => x.End >= startDate);
                            }
                            else if (fieldName == "enddate")
                            {
                                DateTime.TryParse(dates[1].Trim(), out DateTime endDate);
                                endDate = endDate.AddHours(23).AddMinutes(59).AddSeconds(59);
                                query = query.Where(x => x.Start <= endDate);
                            }
                        }
                    }
                }

                // Searching
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(x => x.User.Name.Contains(search)
                        || x.Durasi.ToString().Contains(search)
                        || x.Description.Contains(search));

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
                                case "name": query = query.Where(x => x.User.Name.Contains(value)); break;
                                case "durasi": query = query.Where(x => x.Durasi.ToString().Contains(value)); break;
                                case "description": query = query.Where(x => x.Description.Contains(value)); break;
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
                            case "durasi": query = query.OrderByDescending(x => x.Durasi); break;
                            case "description": query = query.OrderByDescending(x => x.Description); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "name": query = query.OrderBy(x => x.User.Name); break;
                            case "durasi": query = query.OrderBy(x => x.Durasi); break;
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

        public Cuti GetById(Int64 id)
        {
            var context = new EFContext();
            try
            {
                return context.Cutis
                    .Include(x => x.User)
                    .FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
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

        public Company SetCuti(int id, int jatah, User user)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Companies.FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
                
                obj.Cuti = jatah;
                obj.UserUp = user.ID.ToString();
                obj.DateUp = DateTime.Now.AddMinutes(-2);

                context.Companies.Update(obj);
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

        public int SisaCuti(Int64 userID, Int64 companyID)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Attendances.Where(x => x.UserID == userID && x.IsDeleted != true && x.Date.Value.Year == DateTime.Now.Year && x.Status == "Cuti");
                int count = 0;
                if (obj != null)
                {
                    foreach (var o in obj)
                        count++;
                }

                var com = context.Companies.FirstOrDefault(x => x.ID == companyID && x.IsDeleted != true);
                if (com == null)
                    throw new Exception("Company not found!");


                count = (int)(com.Cuti - count);
                return (count);
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