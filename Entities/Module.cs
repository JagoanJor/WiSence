using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Module")]
    public class Module : Entity
    {
        public String Description { get; set; }
        public bool? IsCreate { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsUpdate { get; set; }
        public bool? IsRead { get; set; }
    }
}