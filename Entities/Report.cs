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
        public String NIK { get; set; }
        public String Periode { get; set; }
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
        public String Periode { get; set; }
        public String HariTanggal { get; set; }
        public String In { get; set; }
        public String Out { get; set; }
        public String Status { get; set; }
    }

    [Keyless]
    [Table(name: "vReportAbsensiPerTahun")]
    public class vReportAbsensiPerTahun
    {
        public String Periode { get; set; }
        public int Libur { get; set; }
        public String TotalKerja { get; set; }
        public ICollection<vReportAbsensiListPerTahun> vReportAbsensiListPerTahuns { get; set;  }
    }

    [Keyless]
    [Table(name: "vReportAbsensiListPerTahun")]
    public class vReportAbsensiListPerTahun
    {
        public String Periode { get; set; }
        public Int64 UserID { get; set; }
        public String Nama { get; set; }
        public String Posisi { get; set; }
        public String NIK { get; set; }
        public int Ontime { get; set; }
        public int Terlambat { get; set; }
        public int Absen { get; set; }
        public int Cuti { get; set; }
    }

    [Keyless]
    [Table(name: "vReportCuti")]
    public class vReportCuti
    {
        public String Periode { get; set; }
        public Int64 UserID { get; set; }
        public String Nama { get; set; }
        public String Posisi { get; set; }
        public String NIK { get; set; }
        public int Cuti { get; set; }
        public int JatahCuti { get; set; }
        public ICollection<vReportCutiList> vReportCutiLists { get; set; }
    }

    [Keyless]
    [Table(name: "vReportCutiList")]
    public class vReportCutiList
    {
        public String Periode { get; set; }
        public Int64 UserID { get; set; }
        public String HariTanggal { get; set; }
        public String Description { get; set; }
    }
    
    [Keyless]
    [Table(name: "vReportCutiPerTahun")]
    public class vReportCutiPerTahun
    {
        public String Periode { get; set; }
        public Int64 UserID { get; set; }
        public String Nama { get; set; }
        public String Posisi { get; set; }
        public String NIK { get; set; }
        public int Cuti { get; set; }
        public int JatahCuti { get; set; }
        public int SisaCuti { get; set; }
    }
}