using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Cuti")]
    public class Cuti : Entity
    {
        public Int64 UserID { get; set; }
        public String Description { get; set; }
        public int? Durasi { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public User User { get; set; }
    }
}
