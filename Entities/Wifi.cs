using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "Wifi")]
    public class Wifi : Entity
    {
        public String Name { get; set; }
        public String IPAddress { get; set; }
        public Int64 CompanyID { get; set; }
        public Company Company { get; set; }
    }
}