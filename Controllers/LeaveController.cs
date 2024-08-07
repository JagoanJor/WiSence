﻿using Microsoft.AspNetCore.Mvc;

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

using API.Entities;
using API.Helpers;
using API.Responses;
using API.Services;
using API.Requests;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeaveController : ControllerBase
    {
        private ILeaveService _service;
        private IAuthService _authService;

        public LeaveController(ILeaveService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Get(int limit = 0, int page = 0, string search = "", string sort = "", string filter = "", string date = "")
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Cuti");
                if (!access.IsRead && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                var total = 0;
                var result = _service.GetAll(limit, ref page, ref total, search, sort, filter, date, user);
                var response = new ListResponse<LeaveResponse>(result, total, page);
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
                Trace.WriteLine(message, "CutiController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Cuti");
                if (!access.IsRead && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                var result = _service.GetById(id);
                if (result == null)
                    return BadRequest(new { message = "Invalid ID" });

                var response = new Response<Leave>(result);
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
                Trace.WriteLine(message, "CutiController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] Leave obj)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Cuti");
                if (!access.IsCreate && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                obj.UserIn = user.UserID.ToString();
                obj.DateIn = DateTime.Now.AddHours(7);
                obj.IsDeleted = false;

                var result = _service.Create(obj);
                var response = new Response<Leave>(result);
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
                Trace.WriteLine(message, "CutiController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpPut]
        public IActionResult Edit([FromBody] Leave obj)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Cuti");
                if (!access.IsUpdate && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                obj.UserUp = user.UserID.ToString();

                var result = _service.Edit(obj);
                var response = new Response<Leave>(result);
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
                Trace.WriteLine(message, "CutiController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var access = _authService.CheckRoleAccesibility(user.RoleID, "Cuti");
                if (!access.IsDelete && user.IsAdmin != true)
                    throw new Exception("Tidak diberikan akses!");

                var result = _service.Delete(id, user.UserID.ToString());

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
                Trace.WriteLine(message, "CutiController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpPut("SetCuti")]
        public IActionResult SetCuti(int companyID, int jatah)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var result = _service.SetCuti(companyID, jatah, user);
                var response = new Response<Company>(result);
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
                Trace.WriteLine(message, "CutiController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpGet("SisaCuti")]
        public IActionResult SisaCuti(int userID, int companyID)
        {
            try
            {
                var result = _service.SisaCuti(userID, companyID);
                if (result == null)
                    return BadRequest(new { message = "Invalid ID" });

                var response = new TotalResponse(result);
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
                Trace.WriteLine(message, "CutiController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpPost("Status")]
        public IActionResult Status(int id, String status)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                //obj.UserIn = user.UserID.ToString();

                var result = _service.Status(id, status, user.UserID);
                var response = new Response<Leave>(result);
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
                Trace.WriteLine(message, "WorkOrderController");
                return BadRequest(new { message });
            }
        }
    }
}