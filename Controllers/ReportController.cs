using API.Entities;
using API.Responses;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private IReportService _service;
        public ReportController(IReportService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet("ReportAbsensi")]
        public IActionResult getReportAbsensi(Int64 userID, int bulan, int tahun)
        {
            var result = _service.getReportAbsensi(userID, bulan, tahun);
            var response = new Response<vReportAbsensi>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("ReportAbsensiPerTahun")]
        public IActionResult getReportAbsensiPerTahun(int tahun)
        {
            var result = _service.getReportAbsensiPerTahun(tahun);
            var response = new Response<vReportAbsensiPerTahun>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("ReportCuti")]
        public IActionResult getReportCuti(Int64 userID, int bulan, int tahun)
        {
            var result = _service.getReportCuti(userID, bulan, tahun);
            var response = new Response<ReportCutiResponse>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("ReportCutiPerTahun")]
        public IActionResult getReportCutiPerTahun(int tahun)
        {
            var result = _service.getReportCutiPerTahun(tahun);
            var response = new Response<ReportCutiPerTahunResponse>(result);
            return Ok(response);
        }
    }
}
