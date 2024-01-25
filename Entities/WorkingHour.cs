using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "WorkingHour")]
    public class WorkingHour : Entity
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}