using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Leave")]
    public class Leave : Entity
    {
        [Key]
        public Int64 LeaveID { get; set; }
        public Int64 UserID { get; set; }
        public String Description { get; set; }
        public int? Duration { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public String Status { get; set; }
        public User User { get; set; }
    }
}
