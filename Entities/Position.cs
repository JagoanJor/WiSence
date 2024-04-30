using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "Position")]
    public class Position : Entity
    {
        [Key]
        public Int64 PositionID { get; set; }
        public String Name { get; set; }
        public Int64? DivisionID { get; set; }
        public Division Division { get; set; }
    }
}