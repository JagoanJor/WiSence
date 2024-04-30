using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "Company")]
    public class Company : Entity
    {
        [Key]
        public Int64 CompanyID { get; set; }
        public String Name { get; set; }
        public String Logo { get; set; }
        public int? Leave { get; set; }
        public ICollection<Division> Divisions { get; set; }
        public ICollection<Wifi> Wifis { get; set; }
    }
}