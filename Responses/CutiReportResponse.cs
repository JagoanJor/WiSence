using API.Entities;
using System;
using System.Collections.Generic;

namespace API.Responses
{
    public class CutiReportResponse
    {
        public String Periode { get; set; }
        public ICollection<vReportCutiPerTahun> vReportCutiPerTahuns { get; set; }
        public CutiReportResponse(string periode, ICollection<vReportCutiPerTahun> vReportCutiPerTahuns)
        {
            Periode = periode;
            this.vReportCutiPerTahuns = vReportCutiPerTahuns;
        }
    }
}
