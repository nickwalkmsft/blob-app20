using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using blobapp20.Models;
using Microsoft.Extensions.Options;
using System.IO;
using Microsoft.AspNetCore.Http;
using blobapp20.Helpers;
using System.Linq;

namespace blobapp20.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        // make sure that appsettings.json is filled with the necessary details of the azure storage
        private readonly AzureStorageConfig storageConfig = null;

        public FilesController(IOptions<AzureStorageConfig> config)
        {
            storageConfig = config.Value;
        }

        // GET /api/Files
        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            try
            {
                // TODO put this in startup
                // if (storageConfig.ConnectionString == string.Empty)

                //     return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                // if (storageConfig.ImageContainer == string.Empty)

                //     return BadRequest("Please provide a name for your image container in the azure blob storage");

                List<string> fileUrls = await StorageHelper.GetFileUrls(storageConfig);

                return new ObjectResult(fileUrls);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST /api/Files
        [HttpPost()]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                // TODO put this in startup
                // if (storageConfig.ConnectionString == string.Empty)
                //     return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");
                // if (storageConfig.ImageContainer == string.Empty)
                //     return BadRequest("Please provide a name for your image container in the azure blob storage");

                using (Stream stream = file.OpenReadStream())
                {
                    await StorageHelper.UploadFileToStorage(stream, file.FileName, storageConfig);
                }
                
                return new AcceptedResult();  
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}