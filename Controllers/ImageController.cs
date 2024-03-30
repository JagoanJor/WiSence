using Microsoft.AspNetCore.Mvc;
using API.Helpers;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace API.Controllers
{
    public class ImageController : ControllerBase
    {
        private IWebHostEnvironment _environment;

        public ImageController(IWebHostEnvironment environment)
        {
            this._environment = environment;
        }

        [HttpGet("Image/{fileName}")]
        public FileStreamResult GetFile(string fileName)
        {
            var contentPath = this._environment.ContentRootPath;
            var path = Path.Combine(contentPath, "Uploads");
            return Utils.GetFile(fileName, path);
        }
    }
}