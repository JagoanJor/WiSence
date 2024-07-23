using Microsoft.AspNetCore.Mvc;

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

using API.Entities;
using API.Helpers;
using API.Responses;
using API.Services;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LocationController : ControllerBase
    {
        private IServiceAsync<Location> _service;
        private IAuthService _authService;

        public LocationController(IServiceAsync<Location> service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(int limit = 0, int page = 0, string search = "", string sort = "", string filter = "", string date = "")
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Location Setup");
                if (!access.IsRead && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                var total = 0;
                var result = await _service.GetAllAsync(limit, page, total, search, sort, filter, date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message = inner.Message;
                    inner = inner.InnerException;
                }
                Trace.WriteLine(message, "LocationController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Location Setup");
                if (!access.IsRead && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return BadRequest(new { message = "Invalid ID" });

                var response = new Response<Location>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message = inner.Message;
                    inner = inner.InnerException;
                }
                Trace.WriteLine(message, "LocationController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Location obj)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Location Setup");
                if (!access.IsCreate && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                obj.UserIn = user.UserID.ToString();
                obj.DateIn = DateTime.Now.AddHours(7);
                obj.IsDeleted = false;

                var result = await _service.CreateAsync(obj);
                var response = new Response<Location>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message = inner.Message;
                    inner = inner.InnerException;
                }
                Trace.WriteLine(message, "LocationController");
                return BadRequest(new { message });
            }

        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Edit([FromBody] Location obj)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Location Setup");
                if (!access.IsUpdate && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                obj.UserUp = user.UserID.ToString();

                var result = await _service.EditAsync(obj);
                var response = new Response<Location>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message = inner.Message;
                    inner = inner.InnerException;
                }
                Trace.WriteLine(message, "LocationController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Location Setup");
                if (!access.IsDelete && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                var result = await _service.DeleteAsync(id, user.UserID.ToString());

                var response = new Response<object>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message = inner.Message;
                    inner = inner.InnerException;
                }
                Trace.WriteLine(message, "LocationController");
                return BadRequest(new { message });
            }
        }

    }
}

