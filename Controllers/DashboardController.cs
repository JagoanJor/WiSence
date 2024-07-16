using API.Entities;
using API.Responses;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using API.Helpers;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetTotalKaryawan()
        {
            try
            {
                var result = await _service.TotalKaryawanAsync();
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
        public async Task<IActionResult> GetTotalHadir()
        {
            try
            {
                var result = await _service.TotalHadirAsync();
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
        public async Task<IActionResult> GetTotalPosition()
        {
            try
            {
                var result = await _service.TotalPositionAsync();
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
        public async Task<IActionResult> GetTotalDivision()
        {
            try
            {
                var result = await _service.TotalDivisionAsync();
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
        [HttpGet("JumlahKehadiranHariIni")]
        public async Task<IActionResult> GetJumlahKehadiranHariIni()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var (ontime, terlambat, cuti, absen) = await _service.GetJumlahKehadiranHariIniAsync(user);
                var response = new JumlahKehadiranHariIni(ontime, terlambat, cuti, absen);
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
        [HttpGet("CheckCuti")]
        public async Task<IActionResult> CheckCuti()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var result = await _service.CheckCutiAsync();

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
                Trace.WriteLine(message, "DashboardController");
                return BadRequest(new { message });
            }
        }
    }
}
