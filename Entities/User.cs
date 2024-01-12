using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "User")]
    public class User : Entity
    {
        public Int64 RoleID { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        public Int64 WarehouseID { get; set; }
        public String Password { get; set; }
        public Role Role { get; set; }
    }
}