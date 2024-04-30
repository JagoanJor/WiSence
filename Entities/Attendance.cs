using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Attendance")]
    public class Attendance : Entity
    {
        [Key]
        public Int64 AttendanceID { get; set; }
        public Int64? UserID { get; set; }
        public String Status { get; set; }
        public String Description { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public User User { get; set; }
    }
}