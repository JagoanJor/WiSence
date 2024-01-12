using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "Vendor")]
    public class Vendor : Entity
    {
        public String Code { get; set; }
        public String Name { get; set; }
        public String ExternalID { get; set; }
    }
}