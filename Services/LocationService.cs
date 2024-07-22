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
    public class LocationService : IServiceAsync<Location>
    {
        public async Task<Location> CreateAsync(Location data)
        {
            await using var context = new EFContext();
            try
            {
                var userID = long.Parse(data.UserIn);
                var user = await context.Users.FirstOrDefaultAsync(x => x.UserID == userID && x.IsDeleted != null);

                var pos = await context.Positions.FirstOrDefaultAsync(x => x.PositionID == user.PositionID && x.IsDeleted != true);
                if (pos == null)
                    throw new Exception("Please set user's position!");

                var div = await context.Divisions.FirstOrDefaultAsync(x => x.DivisionID == pos.DivisionID && x.IsDeleted != true);
                if (div == null)
                    throw new Exception("Please set position's division!");

                var com = await context.Companies.FirstOrDefaultAsync(x => x.CompanyID == div.CompanyID && x.IsDeleted != true);
                if (com == null)
                    throw new Exception("Please set division's company!");

                data.CompanyID = user.CompanyID;
                await context.Locations.AddAsync(data);
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
                var obj = await context.Locations.FirstOrDefaultAsync(x => x.LocationID == id && x.IsDeleted != true);
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

        public async Task<Location> EditAsync(Location data)
        {
            await using var context = new EFContext();
            try
            {
                var obj = await context.Locations.FirstOrDefaultAsync(x => x.LocationID == data.LocationID && x.IsDeleted != true);
                if (obj == null) return null;

                obj.Name = data.Name;
                obj.Longtitude = data.Longtitude;
                obj.Latitude = data.Latitude;
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

        public async Task<ListResponse<Location>> GetAllAsync(int limit, int page, int total, string search, string sort, string filter, string date)
        {
            await using var context = new EFContext();
            try
            {
                var query = from a in context.Locations where a.IsDeleted != true select a;
                query = query.Include(l => l.Company);

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
                    query = query.OrderByDescending(x => x.LocationID);
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

                return new ListResponse<Location>(data, total, page);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw;
            }
        }

        public async Task<Location> GetByIdAsync(long id)
        {
            await using var context = new EFContext();
            try
            {
                return await context.Locations
                    .Include(x => x.Company)
                    .FirstOrDefaultAsync(x => x.LocationID == id && x.IsDeleted != true);
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
