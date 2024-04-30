using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Module")]
    public class Module : Entity
    {
        [Key]
        public Int64 ModuleID { get; set; }
        public String Description { get; set; }
    }
}