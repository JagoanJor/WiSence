using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using API.Helpers;
using API.Responses;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class DivisionService : IServiceAsync<Division>
    {
        public async Task<Division> CreateAsync(Division data)
        {
            await using var context = new EFContext();
            try
            {
                await context.Divisions.AddAsync(data);
                await context.SaveChangesAsync();

                return data;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<bool> DeleteAsync(long id, string userID)
        {
            await using var context = new EFContext();
            try
            {
                var obj = await context.Divisions.FirstOrDefaultAsync(x => x.DivisionID == id && x.IsDeleted != true);
                if (obj == null) return false;

                obj.IsDeleted = true;
                obj.UserUp = userID;
                obj.DateUp = DateTime.Now.AddHours(7);

                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<Division> EditAsync(Division data)
        {
            await using var context = new EFContext();
            try
            {
                var obj = await context.Divisions.FirstOrDefaultAsync(x => x.DivisionID == data.DivisionID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.Name = data.Name;
                obj.NumberOfEmployee = data.NumberOfEmployee;
                obj.CompanyID = data.CompanyID;
                obj.UserUp = data.UserUp;
                obj.DateUp = DateTime.Now.AddHours(7);

                await context.SaveChangesAsync();

                return obj;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<ListResponse<Division>> GetAllAsync(int limit, int page, int total, string search, string sort, string filter, string date)
        {
            await using var context = new EFContext();
            try
            {
                var query = from a in context.Divisions where a.IsDeleted != true select a;
                query = query.Include(d => d.Company);

                // Searching
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(x => x.Name.Contains(search)
                        || x.NumberOfEmployee.ToString().Contains(search));

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
                                case "numberofemployee": query = query.Where(x => x.NumberOfEmployee.ToString().Contains(value)); break;
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
                            case "numberofemployee": query = query.OrderByDescending(x => x.NumberOfEmployee.ToString()); break;
                        }
                    }
                    else
                    {
                        switch (orderBy.ToLower())
                        {
                            case "name": query = query.OrderBy(x => x.Name); break;
                            case "numberofemployee": query = query.OrderBy(x => x.NumberOfEmployee.ToString()); break;
                        }
                    }
                }
                else
                {
                    query = query.OrderByDescending(x => x.DivisionID);
                }

                // Get Total Before Limit and Page
                total = await query.CountAsync();

                // Set Limit and Page
                if (limit != 0)
                    query = query.Skip(page * limit).Take(limit);

                // Get Data
                var data = await query.ToListAsync();
                if (data.Count <= 0 && page > 0)
                {
                    page = 0;
                    return await GetAllAsync(limit, page, total, search, sort, filter, date);
                }

                return new ListResponse<Division>(data, total, page);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<Division> GetByIdAsync(long id)
        {
            await using var context = new EFContext();
            try
            {
                return await context.Divisions
                    .Include(x => x.Company)
                    .FirstOrDefaultAsync(x => x.DivisionID == id && x.IsDeleted != true);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }
    }
}
