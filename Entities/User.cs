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
        public String Password { get; set; }
        public String NIK { get; set; }
        public String Gender { get; set; }
        public DateTime DOB { get; set; }
        public String Address { get; set; }
        public String Phone { get; set; }
        public String PositionID { get; set; }
        public Role Role { get; set; }
        public Position Position { get; set; }
    }
}