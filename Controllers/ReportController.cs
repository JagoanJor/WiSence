﻿using API.Entities;
using API.Responses;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    [ApiController]
    [Route("Controller")]
    public class ReportController : ControllerBase
    {
        private IReportService _service;
        public ReportController(IReportService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet("ReportAbsen")]
        public IActionResult getReportAbsen(Int64 userID, int bulan, int tahun)
        {
            var result = _service.getReportAbsensi(userID, bulan, tahun);
            var response = new Response<vReportAbsensi>(result);
            return Ok(response);
        }
    }
}