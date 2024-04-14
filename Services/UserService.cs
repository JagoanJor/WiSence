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
                data.CompanyID = getCompanyID(data.PositionID);

                context.Users.Add(data);
                context.SaveChanges();

                Division(data.PositionID);

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

                // Delete User's cuti data
                context.Cutis.RemoveRange(context.Cutis);

                // Delete User's attendance data
                context.Attendances.RemoveRange(context.Attendances);

                // Delete User's daily task data
                context.DailyTasks.RemoveRange(context.DailyTasks);

                context.SaveChanges();

                Division(obj.PositionID);

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
                obj.Password = data.Password;
                obj.NIK = data.NIK;
                obj.Gender = data.Gender;
                obj.POB = data.POB;
                obj.DOB = data.DOB;
                obj.Address = data.Address;
                obj.Phone = data.Phone;
                obj.PositionID = data.PositionID;
                obj.CompanyID = getCompanyID(data.PositionID);
                obj.IsAdmin = data.IsAdmin;

                obj.UserUp = data.UserUp;
                obj.DateUp = DateTime.Now.AddMinutes(-2);

                if (!string.IsNullOrEmpty(data.Password))
                    obj.Password = Utils.HashPassword(data.Password);

                context.SaveChanges();

                Division(data.PositionID);

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
                query = query.Include("Position");
                query = query.Include("Company");

                // Searching
                if (!string.IsNullOrEmpty(search))
                {
                    decimal number = 0;
                    var isDecimal = decimal.TryParse(search, out number);

                    query = query.Where(x => x.Name.Contains(search)
                        || x.Email.Contains(search)
                        || x.NIK.Contains(search)
                        || x.Gender.Contains(search)
                        || x.Address.Contains(search)
                        || x.Phone.Contains(search)
                        || x.Position.Name.Contains(search)
                        || x.POB.Contains(search)
                        || x.DOB.ToString().Contains(search)
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
                                case "nik": query = query.Where(x => x.NIK.Contains(value)); break;
                                case "gender": query = query.Where(x => x.Gender.Contains(value)); break;
                                case "pob": query = query.Where(x => x.POB.Contains(value)); break;
                                case "dob": query = query.Where(x => x.DOB.ToString().Contains(value)); break;
                                case "address": query = query.Where(x => x.Address.Contains(value)); break;
                                case "phone": query = query.Where(x => x.Phone.Contains(value)); break;
                                case "position": query = query.Where(x => x.Position.Name.Contains(value)); break;
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
                            case "nik": query = query.OrderByDescending(x => x.NIK); break;
                            case "gender": query = query.OrderByDescending(x => x.Gender); break;
                            case "address": query = query.OrderByDescending(x => x.Address); break;
                            case "phone": query = query.OrderByDescending(x => x.Phone); break;
                            case "pob": query = query.OrderByDescending(x => x.POB); break;
                            case "dob": query = query.OrderByDescending(x => x.DOB); break;
                            case "position": query = query.OrderByDescending(x => x.Position.Name); break;
                            case "role": query = query.OrderByDescending(x => x.Role.Name); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "name": query = query.OrderBy(x => x.Name); break;
                            case "email": query = query.OrderBy(x => x.Email); break;
                            case "nik": query = query.OrderBy(x => x.NIK); break;
                            case "gender": query = query.OrderBy(x => x.Gender); break;
                            case "address": query = query.OrderBy(x => x.Address); break;
                            case "phone": query = query.OrderBy(x => x.Phone); break;
                            case "pob": query = query.OrderBy(x => x.POB); break;
                            case "dob": query = query.OrderBy(x => x.DOB); break;
                            case "position": query = query.OrderBy(x => x.Position.Name); break;
                            case "role": query = query.OrderBy(x => x.Role.Name); break;
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
                var obj = context.Users
                    .Include(x => x.Position)
                    .Include(x => x.Role)
                    .Include(x => x.Company)
                    .FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
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

        public Division Division(long? PositionID)
        {
            var context = new EFContext();
            try
            {
                var position = context.Positions.FirstOrDefault(x => x.ID == PositionID && x.IsDeleted != true);
                if (position == null)
                    throw new Exception("No Position Found!");

                var division = context.Divisions.FirstOrDefault(x => x.ID == position.DivisionID && x.IsDeleted != true);
                if (division == null)
                    throw new Exception("Division of the position not found!");
                
                var count = 0;

                var allPosition = context.Positions.Where(x => x.DivisionID == division.ID && x.IsDeleted != true);
                foreach (var all in allPosition)
                {
                    count += context.Users.Where(x => x.PositionID == all.ID && x.IsAdmin != true && x.IsDeleted != true).Count();
                }

                division.NumberOfEmployee = count;

                context.Divisions.Update(division);
                context.SaveChanges();

                return null;
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

        public long? getCompanyID(long? PositionID)
        {
            var context = new EFContext();
            try
            {
                var position = context.Positions.FirstOrDefault(x => x.ID == PositionID && x.IsDeleted != true);
                if (position == null)
                    throw new Exception("No Position Found!");

                var division = context.Divisions.FirstOrDefault(x => x.ID == position.DivisionID && x.IsDeleted != true);
                if (division == null)
                    throw new Exception("Division of the position not found!");

                return division.CompanyID;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if(ex.StackTrace != null)
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

