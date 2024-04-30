using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "User")]
    public class User : Entity
    {
        [Key]
        public Int64 UserID { get; set; }
        public Int64 RoleID { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        public String Password { get; set; }
        public String NIK { get; set; }
        public String Gender { get; set; }
        public String POB { get; set; }
        public DateTime? DOB { get; set; }
        public String Address { get; set; }
        public String Phone { get; set; }
        public String IDCardNumber { get; set; }
        public String Religion { get; set; }
        public String LastEducation { get; set; }
        public String Major { get; set; }
        public String EmployeeType { get; set; }
        public DateTime? StartWork { get; set; }
        public DateTime? EndWork { get; set; }
        public Int64? PositionID { get; set; }
        public Int64? CompanyID { get; set; }
        public Int64? ShiftID { get; set; }
        public Boolean? IsAdmin { get; set; }
        public Role Role { get; set; }
        public Position Position { get; set; }
        public Company Company { get; set; }
        public Shift Shift { get; set; }
    }
}