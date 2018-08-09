using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FileUploader.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace FileUploader.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        private readonly IStorage storage;

        public FilesController(IStorage storage)
        {
            this.storage = storage;
        }

        // GET /api/Files
        // Called by the page when it's first loaded, whenever new files are uploaded, and every
        // five seconds on a timer.
        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            var baseUrl = Request.Path.Value;
            var fileUrls = await storage.GetFileUrls(baseUrl);
            return Ok(fileUrls);            
        }

        // POST /api/Files
        // Called once for each file uploaded.
        [HttpPost()]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            using (Stream stream = file.OpenReadStream())
            {
                await storage.Save(stream, file.FileName);
            }
            
            return Accepted();
        }

        // GET /api/Files/{filename}
        // Called when clicking a link to download a specific file.
        [HttpGet("{filename}")]
        public async Task<IActionResult> Download(string filename)
        {
            var stream = await storage.GetFile(filename);
            return File(stream, "application/octet-stream", filename);
        }
    }
}