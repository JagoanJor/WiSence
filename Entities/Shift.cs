using Org.BouncyCastle.Asn1.Cms;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Shift")]
    public class Shift : Entity
    {
        public DateTime In { get; set; }
        public DateTime Out { get; set; }
        public String Description { get; set; }
    }
}