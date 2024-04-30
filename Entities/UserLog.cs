using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table(name: "UserLogs")]
    public class UserLog : Entity
    {
        [Key]
        public Int64 UserLogsID { get; set; }
        public Nullable<Int64> ObjectID { get; set; }
        public Nullable<Int64> ModuleID { get; set; }
        public Int64 UserID { get; set; }
        public String Description { get; set; }
        public DateTime TransDate { get; set; }
    }
}