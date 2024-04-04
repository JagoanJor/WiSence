using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Keyless]
    [Table(name: "vReportAbsensi")]
    public class vReportAbsensi
    {
        public String Nama { get; set; }
        public Int64 UserID { get; set; }
        public String Posisi { get; set; }
        public String Bulan { get; set; }
        public int Kerja { get; set; }
        public int Libur { get; set; }
        public String TotalKerja { get; set; }
        public ICollection<vReportAbsensiList> vReportAbsensiLists { get; set; }
    }

    [Keyless]
    [Table(name: "vReportAbsensiList")]
    public class vReportAbsensiList
    {
        public Int64 UserID { get; set; }
        public String Bulan { get; set; }
        public String HariTanggal { get; set; }
        public String In { get; set; }
        public String Out { get; set; }
        public String Status { get; set; }
        public int TotalHadir { get; set; }
    }
}