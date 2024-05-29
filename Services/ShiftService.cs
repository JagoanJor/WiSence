using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using API.Entities;
using API.Helpers;

using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class ShiftService : IService<Shift>
    {
        public Shift Create(Shift data)
        {
            var context = new EFContext();
            try
            {
                context.Shifts.Add(data);
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
                var obj = context.Shifts.FirstOrDefault(x => x.ShiftID == id && x.IsDeleted != true);
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

        public Shift Edit(Shift data)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Shifts.FirstOrDefault(x => x.ShiftID == data.ShiftID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.ClockIn = data.ClockIn;
                obj.ClockOut = data.ClockOut;
                obj.Description = data.Description;
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

        public IEnumerable<Shift> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Shifts where a.IsDeleted != true select a;

                // Searching
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(x => x.ClockIn.ToString().Contains(search)
                        || x.ClockOut.ToString().Contains(search)
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
                                case "clockin": query = query.Where(x => x.ClockIn.ToString().Contains(value)); break;
                                case "clockout": query = query.Where(x => x.ClockOut.ToString().Contains(value)); break;
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
                            case "clockin": query = query.OrderByDescending(x => x.ClockIn); break;
                            case "clockout": query = query.OrderByDescending(x => x.ClockOut); break;
                            case "description": query = query.OrderByDescending(x => x.Description); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "clockin": query = query.OrderBy(x => x.ClockIn); break;
                            case "clockout": query = query.OrderBy(x => x.ClockOut); break;
                            case "description": query = query.OrderBy(x => x.Description); break;
                        }
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.ShiftID);
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

        public Shift GetById(Int64 id)
        {
            var context = new EFContext();
            try
            {
                return context.Shifts.FirstOrDefault(x => x.ShiftID == id && x.IsDeleted != true);
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

