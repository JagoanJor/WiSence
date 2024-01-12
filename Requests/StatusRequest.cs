using System;

namespace API.Requests
{
    public class StatusRequest
    {
        public Int64 ID { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class ExternalIDRequest
    {
        public Int64 ID { get; set; }
        public string ExternalID { get; set; }
    }

    public class SyncDateRequest
    {
        public Int64 ID { get; set; }
    }

    public class SyncDateRequestChecklist
    {
        public Int64 ID { get; set; }
        public string Status { get; set; }
    }
}