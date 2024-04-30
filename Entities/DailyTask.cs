using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    [Table(name: "DailyTask")]
    public class DailyTask : Entity
    {
        [Key]
        public Int64 DailyTaskID { get; set; }
        public Int64 UserID { get; set; }
        public String Task { get; set; }
        public DateTime Date { get; set; }
        public User User { get; set; }
    }
}