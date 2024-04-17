﻿using Org.BouncyCastle.Asn1.Cms;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Shift")]
    public class Shift : Entity
    {
        public DateTime ClockIn { get; set; }
        public DateTime ClockOut { get; set; }
        public String Description { get; set; }
    }
}