using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "Division")]
    public class Division : Entity
    {
        public String Name { get; set; }
        public int? NumberOfEmployee { get; set; }
        public Int64? CompanyID { get; set; }
        public Company Company { get; set; }
        public ICollection<Position> Positions { get; set; }
    }
}