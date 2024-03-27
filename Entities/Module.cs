using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Module")]
    public class Module : Entity
    {
        public String Description { get; set; }
        public Boolean? IsCreate { get; set; }
        public Boolean? IsDelete { get; set; }
        public Boolean? IsUpdate { get; set; }
        public Boolean? IsRead { get; set; }
    }
}