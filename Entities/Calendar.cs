using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Calendar")]
    public class Calendar : Entity
    {
        public String Description { get; set; }
        public DateTime Holiday { get; set; }
    }
}
