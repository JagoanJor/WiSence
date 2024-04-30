using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Calendar")]
    public class Calendar : Entity
    {
        [Key]
        public Int64 CalendarID { get; set; }
        public String Description { get; set; }
        public DateTime Holiday { get; set; }
    }
}
