using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "Wifi")]
    public class Wifi : Entity
    {
        [Key]
        public Int64 WifiID { get; set; }
        public String Name { get; set; }
        public String IPAddress { get; set; }
        public Int64? CompanyID { get; set; }
        public Company Company { get; set; }
    }
}