using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "Location")]
    public class Location : Entity
    {
        [Key]
        public Int64 LocationID { get; set; }
        public String Name { get; set; }
        public Double Longtitude { get; set; }
        public Double Latitude { get; set; }
        public Int64? CompanyID { get; set; }
        public Company Company { get; set; }
    }
}