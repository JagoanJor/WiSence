using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

using API.Entities;
using API.Helpers;

namespace API.Services
{
    public class UserService : IService<User>
    {
        public User Create(User data)
        {
            var context = new EFContext();
            try
            {
                if (string.IsNullOrEmpty(data.Password)) data.Password = "password";
                data.Password = Utils.HashPassword(data.Password);

                context.Users.Add(data);
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
                var obj = context.Users.FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
                if (obj == null) return false;

                obj.IsDeleted = true;
                obj.UserUp = userID.ToString();
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

        public User Edit(User data)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Users.FirstOrDefault(x => x.ID == data.ID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.RoleID = data.RoleID;

                obj.Name = data.Name;
                obj.Email = data.Email;

                obj.UserUp = data.UserUp;
                obj.DateUp = DateTime.Now.AddMinutes(-2);

                if (!string.IsNullOrEmpty(data.Password))
                    obj.Password = Utils.HashPassword(data.Password);

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


        public IEnumerable<User> GetAll(Int32 limit, ref Int32 page, ref Int32 total, String search, String sort, String filter, String date)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.Users where a.IsDeleted != true select a;
                query = query.Include("Role");

                // Searching
                if (!string.IsNullOrEmpty(search))
                {
                    decimal number = 0;
                    var isDecimal = decimal.TryParse(search, out number);

                    query = query.Where(x => x.Name.Contains(search)
                        || x.Email.Contains(search)
                        || x.Role.Name.Contains(search));
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
                                case "name": query = query.Where(x => x.Name.Contains(value)); break;
                                case "email": query = query.Where(x => x.Email.Contains(value)); break;
                                case "role": query = query.Where(x => x.Role.Name.Contains(value)); break;
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
                            case "email": query = query.OrderByDescending(x => x.Email); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "name": query = query.OrderBy(x => x.Name); break;
                            case "email": query = query.OrderBy(x => x.Email); break;
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

                foreach (var item in data)
                    item.Password = "";

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

        public User GetById(Int64 id)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Users.FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
                if (obj != null) obj.Password = "";
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
    }
}

