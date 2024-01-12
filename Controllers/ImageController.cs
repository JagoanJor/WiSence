using Microsoft.AspNetCore.Mvc;
using API.Helpers;

namespace API.Controllers
{
    public class FileController : ControllerBase
    {
        [HttpGet("Image/{fileName}")]
        public FileStreamResult GetFile(string fileName)
        {
            return Utils.GetFile(fileName);
        }
    }
}