using API.Entities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace API.Responses
{
    public class ReportAbsensiPerTahunResponse
    {
        public String Periode { get; set; }
        public int Libur { get; set; }
        public String TotalKerja { get; set; }
        public ICollection<vReportAbsensiListPerTahun> vReportAbsensiListPerTahun { get; set; }
        public ReportAbsensiPerTahunResponse (String periode, int libur, string totalKerja, ICollection<vReportAbsensiListPerTahun> vReportAbsensiListPerTahuns)
        {
            Periode = periode;
            Libur = libur;
            TotalKerja = totalKerja;
            this.vReportAbsensiListPerTahun = vReportAbsensiListPerTahuns;
        }
    }
}
