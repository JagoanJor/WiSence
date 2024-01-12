using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using API.Entities;
using API.Helpers;

namespace API.Services
{
    public class RoleService : IService<Role>
    {
        public Role Create(Role data)
        {
            var context = new EFContext();
            try
            {
                context.Roles.Add(data);
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
                var obj = context.Roles
                    .Include(x => x.RoleDetails.Where(y => y.IsDeleted == null))
                    .Where(x => x.ID == id)
                    .FirstOrDefault();
                if (obj == null) return false;

                obj.IsDeleted = true;
                obj.UserUp = userID;
                obj.DateUp = DateTime.Now.AddMinutes(-2);

                foreach (var role in obj.RoleDetails)
                {
                    role.IsDeleted = true;
                    role.UserUp = userID;
                    role.DateUp = DateTime.Now.AddMinutes(-2);
                }

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

        public Role Edit(Role data)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Roles
                    .Include(x => x.RoleDetails.Where(y => y.IsDeleted == null))
                    .Where(x => x.ID == data.ID).FirstOrDefault();
                if (obj == null) return null;

                obj.Name = data.Name;
                obj.UserUp = data.UserUp;
                obj.DateUp = DateTime.Now.AddMinutes(-2);

                // Update & Set IsDeleted on Existing
                if (obj.RoleDetails != null)
                {
                    foreach (var role in obj.RoleDetails)
                    {
                        var newRole = data.RoleDetails.FirstOrDefault(x => x.ModuleID == role.ModuleID);
                        if (newRole == null)
                            role.IsDeleted = true;
                        else
                        {
                            role.IsCreate = newRole.IsCreate;
                            role.IsRead = newRole.IsRead;
                            role.IsUpdate = newRole.IsUpdate;
                            role.IsDelete = newRole.IsDelete;
                        }
                        role.UserUp = data.UserUp;
                        role.DateUp = DateTime.Now.AddMinutes(-2);
                    }
                }

                // New Detail
                if (data.RoleDetails != null)
                {
                    foreach (var role in data.RoleDetails)
                    {
                        var oldRole = obj.RoleDetails.FirstOrDefault(x => x.ModuleID == role.ModuleID);
                        if (oldRole == null)
                        {
                            var newRole = new RoleDetail();
                            newRole.UserIn = data.UserUp;
                            newRole.DateIn = DateTime.Now.AddMinutes(-2);
                            newRole.ModuleID = role.ModuleID;
                            newRole.IsCreate = role.IsCreate;
                            newRole.IsRead = role.IsRead;
                            newRole.IsUpdate = role.IsUpdate;
                            newRole.IsDelete = role.IsDelete;
                            obj.RoleDetails.Add(newRole);
                        }
                    }
                }

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

        public IEnumerable<Role> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Roles where a.IsDeleted != true select a;
                query = query.Include("RoleDetails");

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

        public Role GetById(Int64 id)
        {
            var context = new EFContext();
            try
            {
                return context.Roles
                    .Include(x => x.RoleDetails.Where(y => y.IsDeleted == null))
                    .Where(x => x.ID == id && x.IsDeleted != true)
                    .FirstOrDefault();
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

