﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using API.Entities;
using API.Helpers;

namespace API.Services
{
    public class ModuleService : IService<Module>
    {
        public Module Create(Module data)
        {
            var context = new EFContext();
            try
            {
                context.Modules.Add(data);
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
                var obj = context.Modules.FirstOrDefault(x => x.ModuleID == id && x.IsDeleted != true);
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

        public Module Edit(Module data)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Modules.FirstOrDefault(x => x.ModuleID == data.ModuleID && x.IsDeleted != true);
                if (obj == null) return null;

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

        public IEnumerable<Module> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Modules where a.IsDeleted != true select a;

                // Searching
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(x => x.Description.Contains(search));

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
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "description": query = query.OrderBy(x => x.Description); break;
                        }
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.ModuleID);
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

        public Module GetById(Int64 id)
        {
            var context = new EFContext();
            try
            {
                return context.Modules.FirstOrDefault(x => x.ModuleID == id && x.IsDeleted != true);
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

