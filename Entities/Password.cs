using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "Passwords")]
    public class Password : Entity
    {
        public Int64 UserID { get; set; }
        public String UniqueCode { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
}