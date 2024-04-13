using API.Entities;
using System;

namespace API.Responses
{
    public class CutiResponse : Cuti
    {
        public int? SisaCuti { get; set; }
        public CutiResponse(Int64 userID, String description, int? durasi, DateTime? start, DateTime? end, String status, int? sisaCuti)
        {
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
