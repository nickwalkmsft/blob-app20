using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using blobapp20.Models;
using Microsoft.Extensions.Options;

using System.IO;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using blobapp20.Helpers;

namespace blobapp20.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        // make sure that appsettings.json is filled with the necessary details of the azure storage
        private readonly AzureStorageConfig storageConfig = null;

        public ImagesController(IOptions<AzureStorageConfig> config)
        {
            storageConfig = config.Value;
        }

        // POST /api/images/upload
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            bool isUploaded = false;

            try
            {

                if (files.Count == 0)

                    return BadRequest("No files received from the upload");

                if (storageConfig.ConnectionString == string.Empty)

                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)

                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                {
                    // if (storageConfig.ThumbnailContainer != string.Empty)

                    //     return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);

                    // else

                        return new AcceptedResult();
                }
                else

                    return BadRequest("Look like the image couldnt upload to the storage");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/images/thumbnails
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {

            try
            {
                if (storageConfig.ConnectionString == string.Empty)

                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)

                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);

                return new ObjectResult(thumbnailUrls);
            
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}