using API.Entities;
using API.Responses;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> getReportAbsensi(Int64 userID, int bulan, int tahun)
        {
            var result = await _service.getReportAbsensiAsync(userID, bulan, tahun);
            var response = new Response<vReportAbsensi>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("ReportAbsensiPerTahun")]
        public async Task<IActionResult> getReportAbsensiPerTahun(int tahun)
        {
            var result = await _service.getReportAbsensiPerTahunAsync(tahun);
            var response = new Response<ReportAbsensiPerTahunResponse>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("ReportCuti")]
        public async Task<IActionResult> getReportCuti(Int64 userID, int bulan, int tahun)
        {
            var result = await _service.getReportCutiAsync(userID, bulan, tahun);
            var response = new Response<ReportCutiResponse>(result);
            return Ok(response);
        }

        [Authorize]
        [HttpGet("ReportCutiPerTahun")]
        public async Task<IActionResult> getReportCutiPerTahun(int tahun)
        {
            var result = await _service.getReportCutiPerTahunAsync(tahun);
            var response = new Response<ReportCutiPerTahunResponse>(result);
            return Ok(response);
        }
    }
}
