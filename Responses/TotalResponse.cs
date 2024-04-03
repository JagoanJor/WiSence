namespace API.Responses
{
    public class TotalResponse
    {
        public int Total { get; set; }
        public TotalResponse(int total)
        {
            Total = total;
        }
    }
}
