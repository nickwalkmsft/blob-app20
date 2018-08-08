using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using FileUploader.Helpers;

namespace FileUploader.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        private readonly IStorageHelper storageHelper;

        public FilesController(IStorageHelper storageHelper)
        {
            this.storageHelper = storageHelper;
        }

        // GET /api/Files
        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            var baseUrl = Request.Path.Value;
            List<string> fileUrls = await storageHelper.GetFileUrls(baseUrl);
            return Ok(fileUrls);            
        }

        // POST /api/Files
        [HttpPost()]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            using (Stream stream = file.OpenReadStream())
            {
                await storageHelper.UploadFileToStorage(stream, file.FileName);
            }
            
            return Accepted();
        }

        // GET /api/Files/{filename}
        [HttpGet("{filename}")]
        public async Task<IActionResult> GetFile(string filename)
        {
            var stream = await storageHelper.GetFile(filename);
            return File(stream, "application/octet-stream", filename);
        }
    }
}