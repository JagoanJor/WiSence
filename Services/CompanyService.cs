using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using API.Entities;
using API.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public interface ICompanyService<T>
    {
        IEnumerable<T> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date);
        T GetById(Int64 id, String baseUrl);
        T Create(T data);
        T Edit(T data, String baseUrl);
        bool Delete(Int64 id, String userID);
        T WorkingHour();
        T SetWorkingHour(Company data);
    }
    public class CompanyService : ICompanyService<Company>
    {
        private IWebHostEnvironment _environment;
        public CompanyService(IWebHostEnvironment environment)
        {
            this._environment = environment;
        }
        public Company Create(Company data)
        {
            var context = new EFContext();
            try
            {
                var contentPath = this._environment.ContentRootPath;
                var path = Path.Combine(contentPath, "Uploads");
                if (Utils.IsBase64String(data.Logo))
                    data.Logo = Utils.SaveFile(data.Logo, path);

                context.Companies.Add(data);
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
                var obj = context.Companies.FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
                if (obj == null) return false;

                var contentPath = this._environment.ContentRootPath;
                var path = Path.Combine(contentPath, "Uploads");
                if (obj.Logo != null && obj.Logo != "")
                    Utils.DeleteImage(obj.Logo, path);

                obj.IsDeleted = true;
                obj.UserUp = userID;
                obj.DateUp = DateTime.Now.AddMinutes(-2);

                // Delete user data (Except admin)
                var user = context.Users.Where(x => x.IsAdmin != true && x.IsDeleted != true);
                if (user != null)
                    context.Users.RemoveRange(user);

                /*// Delete attendance data
                context.Attendances.RemoveRange(context.Attendances);

                // Delete cuti data
                context.Cutis.RemoveRange(context.Cutis);

                // Delete daily task data
                context.DailyTasks.RemoveRange(context.DailyTasks);

                // Delete division data
                context.Divisions.RemoveRange(context.Divisions);

                // Delete position data
                context.Positions.RemoveRange(context.Positions);

                // Delete userlog data
                context.UserLogs.RemoveRange(context.UserLogs);

                // Delete wifi data
                context.Wifis.RemoveRange(context.Wifis);*/

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

        public Company Edit(Company data, String baseUrl)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Companies.FirstOrDefault(x => x.ID == data.ID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.Name = data.Name;

                if (data.Logo != "" && data.Logo != null)
                {
                    if (data.Logo.Contains(baseUrl))
                        data.Logo = data.Logo.Replace(baseUrl, "");
                }

                if (Utils.IsBase64String(data.Logo))
                {
                    var contentPath = this._environment.ContentRootPath;
                    var path = Path.Combine(contentPath, "Uploads");
                    obj.Logo = Utils.SaveFile(data.Logo, path);
                }
                else
                    obj.Logo = data.Logo;

                obj.Start = data.Start;
                obj.End = data.End;
                obj.Cuti = data.Cuti;

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

        public IEnumerable<Company> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Companies where a.IsDeleted != true select a;

                // Searching
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(x => x.Name.Contains(search));

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
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "name": query = query.OrderBy(x => x.Name); break;
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

        public Company GetById(Int64 id, String baseUrl)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Companies where a.IsDeleted != true && a.ID == id select a;
                query = query.Include("Wifis");

                var data = query.FirstOrDefault();

                if (data.Logo != null)
                    data.Logo = string.Format("{0}/Image/{1}", baseUrl, data.Logo);

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

        public Company WorkingHour()
        {
            var context = new EFContext();
            try
            {
                return context.Companies.FirstOrDefault(x => x.IsDeleted != true);
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
        public Company SetWorkingHour(Company data)
        {
            var context = new EFContext();
            try
            {
                var company = context.Companies.FirstOrDefault(x => x.ID == data.ID && x.IsDeleted != true);

                company.Start = data.Start;
                company.End = data.End;
                company.UserUp = data.UserUp;
                company.DateUp = data.DateUp;

                context.Companies.Update(company);
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

