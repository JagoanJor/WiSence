using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using API.Entities;
using API.Helpers;

using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class DailyTaskService : IService<DailyTask>
    {
        public DailyTask Create(DailyTask data)
        {
            var context = new EFContext();
            try
            {
                context.DailyTasks.Add(data);
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
                var obj = context.DailyTasks.FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
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

        public DailyTask Edit(DailyTask data)
        {
            var context = new EFContext();
            try
            {
                var obj = context.DailyTasks.FirstOrDefault(x => x.ID == data.ID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.UserID = data.UserID;
                obj.Task = data.Task;
                obj.Date = data.Date;
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

        public IEnumerable<DailyTask> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.DailyTasks where a.IsDeleted != true select a;
                query = query.Include("User");

                // Searching
                if (!string.IsNullOrEmpty(search))
                {
                    if (DateTime.TryParse(search, out DateTime searchDate))
                        query = query.Where(x => x.Date == searchDate
                        || (x.Date.Month == searchDate.Month && x.Date.Day == searchDate.Day));
                    else
                        query = query.Where(x => x.UserID.ToString().Contains(search)
                        || x.Task.Contains(search));
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
                                case "userid": query = query.Where(x => x.UserID.ToString().Contains(value)); break;
                                case "task": query = query.Where(x => x.Task.Contains(value)); break;
                                case "date":
                                    DateTime.TryParse(value, out DateTime searchDate);
                                    query = query.Where(x => x.Date.Date == searchDate.Date);
                                    break;
                                case "nik": query = query.Where(x => x.User.NIK.Contains(value)); break;
                                case "name": query = query.Where(x => x.User.Name.Contains(value)); break;
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
                            case "userid": query = query.OrderByDescending(x => x.UserID.ToString()); break;
                            case "task": query = query.OrderByDescending(x => x.Task); break;
                            case "date": query = query.OrderByDescending(x => x.Date); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "userid": query = query.OrderBy(x => x.UserID.ToString()); break;
                            case "task": query = query.OrderBy(x => x.Task); break;
                            case "date": query = query.OrderBy(x => x.Date); break;
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

        public DailyTask GetById(Int64 id)
        {
            var context = new EFContext();
            try
            {
                return context.DailyTasks
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
    }
}

