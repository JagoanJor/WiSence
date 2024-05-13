using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using API.Entities;
using API.Helpers;

using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class CalendarService : IService<Calendar>
    {
        public Calendar Create(Calendar data)
        {
            var context = new EFContext();
            try
            {
                context.Calendars.Add(data);
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
                var obj = context.Calendars.FirstOrDefault(x => x.CalendarID == id && x.IsDeleted != true);
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

        public Calendar Edit(Calendar data)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Calendars.FirstOrDefault(x => x.CalendarID == data.CalendarID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.Description = data.Description;
                obj.Holiday = data.Holiday;
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

        public IEnumerable<Calendar> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Calendars where a.IsDeleted != true select a;

                // Searching
                if (!string.IsNullOrEmpty(search))
                {
                    if (DateTime.TryParse(search, out DateTime searchDate))
                        query = query.Where(x => x.Holiday == searchDate || (x.Holiday.Month == searchDate.Month && x.Holiday.Year == searchDate.Year));
                    else
                        query = query.Where(x => x.Description.Contains(search));
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
                                case "description": query = query.Where(x => x.Description.Contains(value)); break;
                                case "holiday":
                                    DateTime.TryParse(value, out DateTime searchDate);
                                    query = query.Where(x => x.Holiday.Date == searchDate.Date);
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
                            case "description": query = query.OrderByDescending(x => x.Description); break;
                            case "holiday": query = query.OrderByDescending(x => x.Holiday); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "description": query = query.OrderBy(x => x.Description); break;
                            case "holiday": query = query.OrderBy(x => x.Holiday); break;
                        }
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.CalendarID);
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

        public Calendar GetById(Int64 id)
        {
            var context = new EFContext();
            try
            {
                return context.Calendars.FirstOrDefault(x => x.CalendarID == id && x.IsDeleted != true);
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

