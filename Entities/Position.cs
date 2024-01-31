using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "Position")]
    public class Position : Entity
    {
        public String Name { get; set; }
        public Int64? DivisionID { get; set; }
        public Division Division { get; set; }
    }
}