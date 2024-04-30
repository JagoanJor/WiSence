using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Passwords")]
    public class Password : Entity
    {
        [Key]
        public Int64 PasswordID { get; set; }
        public Int64 UserID { get; set; }
        public String UniqueCode { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
}