using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Module")]
    public class Module : Entity
    {
        public String Description { get; set; }
    }
}