﻿using Microsoft.AspNetCore.Mvc;

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

using API.Entities;
using API.Helpers;
using API.Responses;
using API.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalendarController : ControllerBase
    {
        private IService<Calendar> _service;

        public CalendarController(IService<Calendar> service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Get(int limit = 0, int page = 0, string search = "", string sort = "", string filter = "", string date = "")
        {
            try
            {
                var total = 0;
                var result = _service.GetAll(limit, ref page, ref total, search, sort, filter, date);
                var response = new ListResponse<Calendar>(result, total, page);
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
                Trace.WriteLine(message, "CalendarController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var result = _service.GetById(id);
                if (result == null)
                    return BadRequest(new { message = "Invalid ID" });

                var response = new Response<Calendar>(result);
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
                Trace.WriteLine(message, "CalendarController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create([FromBody] Calendar obj)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                obj.UserIn = user.UserID.ToString();
                obj.DateIn = DateTime.Now.AddHours(7);
                obj.IsDeleted = false;

                var result = _service.Create(obj);
                var response = new Response<Calendar>(result);
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
                Trace.WriteLine(message, "CalendarController");
                return BadRequest(new { message });
            }

        }

        [Authorize]
        [HttpPut]
        public IActionResult Edit([FromBody] Calendar obj)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                obj.UserUp = user.UserID.ToString();

                var result = _service.Edit(obj);
                var response = new Response<Calendar>(result);
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
                Trace.WriteLine(message, "CalendarController");
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
                Trace.WriteLine(message, "CalendarController");
                return BadRequest(new { message });
            }
        }

    }
}

