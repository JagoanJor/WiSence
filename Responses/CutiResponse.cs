using API.Entities;
using System;

namespace API.Responses
{
    public class CutiResponse
    {
        public Int64 ID { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateUp { get; set; }
        public String UserIn { get; set; }
        public String UserUp { get; set; }
        public Boolean? IsDeleted { get; set; }
        public Int64 UserID { get; set; }
        public String Description { get; set; }
        public int? Durasi { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public String Status { get; set; }
        public int? SisaCuti { get; set; }
        public CutiResponse(Int64 id, DateTime? dateIn, DateTime? dateUp, String userIn, String userUp, Boolean? isDeleted, Int64 userID, String description, int? durasi, DateTime? start, DateTime? end, String status, int? sisaCuti)
        {
            ID = id;
            DateIn = dateIn;
            DateUp = dateUp;
            UserIn = userIn;
            UserUp = userUp;
            IsDeleted = isDeleted;
            UserID = userID;
            Description = description;
            Durasi = durasi;
            Start = start;
            End = end;
            Status = status;
            SisaCuti = sisaCuti;
        }
    }
}
