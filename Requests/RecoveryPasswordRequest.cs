namespace API.Requests
{
    public class RecoveryPasswordRequest
    {
        public string Code { get; set; }
        public string Password { get; set; }
    }
}