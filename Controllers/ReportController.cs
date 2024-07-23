using API.Entities;
using API.Helpers;
using API.Responses;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private IReportService _service;
        private IAuthService _authService;
        public ReportController(IReportService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }

        [Authorize]
        [HttpGet("ReportAbsensi")]
        public async Task<IActionResult> getReportAbsensi(Int64 userID, int bulan, int tahun)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var user = (User)null;
            if (token != null)
                user = Utils.UserFromToken(token);

            if (user == null)
                return BadRequest(new { message = "Invalid Token" });

            var access = _authService.CheckRoleAccesibility(user.RoleID, "Reports");
            if (!access.IsRead && user.IsAdmin != true)
                throw new Exception("Tidak diberikan akses!");

            var result = await _service.getReportAbsensiAsync(userID, bulan, tahun);
            var response = new Response<vReportAbsensi>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("ReportAbsensiPerTahun")]
        public async Task<IActionResult> getReportAbsensiPerTahun(int tahun)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var user = (User)null;
            if (token != null)
                user = Utils.UserFromToken(token);

            if (user == null)
                return BadRequest(new { message = "Invalid Token" });

            var access = _authService.CheckRoleAccesibility(user.RoleID, "Reports");
            if (!access.IsRead && user.IsAdmin != true)
                throw new Exception("Tidak diberikan akses!");

            var result = await _service.getReportAbsensiPerTahunAsync(tahun);
            var response = new Response<ReportAbsensiPerTahunResponse>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("ReportCuti")]
        public async Task<IActionResult> getReportCuti(Int64 userID, int bulan, int tahun)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var user = (User)null;
            if (token != null)
                user = Utils.UserFromToken(token);

            if (user == null)
                return BadRequest(new { message = "Invalid Token" });

            var access = _authService.CheckRoleAccesibility(user.RoleID, "Reports");
            if (!access.IsRead && user.IsAdmin != true)
                throw new Exception("Tidak diberikan akses!");

            var result = await _service.getReportCutiAsync(userID, bulan, tahun);
            var response = new Response<ReportCutiResponse>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("ReportCutiPerTahun")]
        public async Task<IActionResult> getReportCutiPerTahun(int tahun)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var user = (User)null;
            if (token != null)
                user = Utils.UserFromToken(token);

            if (user == null)
                return BadRequest(new { message = "Invalid Token" });

            var access = _authService.CheckRoleAccesibility(user.RoleID, "Reports");
            if (!access.IsRead && user.IsAdmin != true)
                throw new Exception("Tidak diberikan akses!");

            var result = await _service.getReportCutiPerTahunAsync(tahun);
            var response = new Response<ReportCutiPerTahunResponse>(result);
            return Ok(response);
        }
    }
}
