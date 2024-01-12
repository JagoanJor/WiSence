using Microsoft.AspNetCore.Http;

namespace API.Requests
{
    public class UploadItemRequest
    {
        public IFormFile File { get; set; }
    }
}