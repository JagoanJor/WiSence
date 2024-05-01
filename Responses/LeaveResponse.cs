using API.Entities;
using System;

namespace API.Responses
{
    public class LeaveResponse
    {
        public Int64 LeaveID { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateUp { get; set; }
        public String UserIn { get; set; }
        public String UserUp { get; set; }
        public Boolean? IsDeleted { get; set; }
        public Int64 UserID { get; set; }
        public String Description { get; set; }
        public int? Duration { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public String Status { get; set; }
        public int? LeaveAllowance { get; set; }
        public User User { get; set; }
        public LeaveResponse(Int64 leaveID, DateTime? dateIn, DateTime? dateUp, String userIn, String userUp, Boolean? isDeleted, Int64 userID, String description, int? duration, DateTime? start, DateTime? end, String status, int? leaveAllowance, User user)
        {
            LeaveID = leaveID;
            DateIn = dateIn;
            DateUp = dateUp;
            UserIn = userIn;
            UserUp = userUp;
            IsDeleted = isDeleted;
            UserID = userID;
            Description = description;
            Duration = duration;
            Start = start;
            End = end;
            Status = status;
            LeaveAllowance = leaveAllowance;
            User = user;
        }
    }
}
