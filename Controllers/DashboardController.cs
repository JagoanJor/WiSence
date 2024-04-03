using API.Entities;
using API.Responses;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : ControllerBase
    {
        private IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet("TotalKaryawan")]
        public IActionResult GetTotalKaryawan()
        {
            try
            {
                var result = _service.TotalKaryawan();
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
                Trace.WriteLine(message, "DashboardController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpGet("TotalHadir")]
        public IActionResult GetTotalHadir()
        {
            try
            {
                var result = _service.TotalHadir();
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
                Trace.WriteLine(message, "DashboardController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpGet("TotalPosition")]
        public IActionResult GetTotalPosition()
        {
            try
            {
                var result = _service.TotalPosition();
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
                Trace.WriteLine(message, "DashboardController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpGet("TotalDivision")]
        public IActionResult GetTotalDivision()
        {
            try
            {
                var result = _service.TotalDivision();
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
                Trace.WriteLine(message, "DashboardController");
                return BadRequest(new { message });
            }
        }
    }
}
