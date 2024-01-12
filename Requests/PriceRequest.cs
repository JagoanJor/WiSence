using System;

namespace API.Requests
{
    public class PriceRequest
    {
        public Decimal Weight { get; set; }
        public Decimal Volume { get; set; }
        public Decimal Size { get; set; }
        public Decimal Distance { get; set; }
        public Int64 FleetCategoryID { get; set; }
        public Int64 CustomerID { get; set; }
    }
}