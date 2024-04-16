using API.Entities;
using System;
using System.Collections.Generic;

namespace API.Responses
{
    public class ReportCutiResponse
    {
        public String Periode { get; set; }
        public Int64 UserID { get; set; }
        public String Nama { get; set; }
        public String Posisi { get; set; }
        public String NIK { get; set; }
        public int Cuti { get; set; }
        public int JatahCuti { get; set; }
        public int SisaCuti { get; set; }
        public ICollection<vReportCutiList> vReportCutiLists { get; set; }
        public ReportCutiResponse(String periode, Int64 userID, String nama, String posisi, String nIK, int cuti, int jatahCuti, int sisaCuti, ICollection<vReportCutiList> vReportCutiLists)
        {
            Periode = periode;
            UserID = userID;
            Nama = nama;
            Posisi = posisi;
            NIK = nIK;
            Cuti = cuti;
            JatahCuti = jatahCuti;
            SisaCuti = sisaCuti;
            this.vReportCutiLists = vReportCutiLists;
        }
    }
    public class ReportCutiPerTahunResponse
    {
        public String Periode { get; set; }
        public ICollection<vReportCutiPerTahun> vReportCutiPerTahuns { get; set; }
        public ReportCutiPerTahunResponse(String periode, ICollection<vReportCutiPerTahun> vReportCutiPerTahuns)
        {
            Periode = periode;
            this.vReportCutiPerTahuns = vReportCutiPerTahuns;
        }
    }
}
