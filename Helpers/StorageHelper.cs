using blobapp20.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace blobapp20.Helpers
{
    public static class StorageHelper
    {

        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static Task UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
        {
            // TODO new comment
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConfig.ConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Upload the file
            return blockBlob.UploadFromStreamAsync(fileStream);
        }

        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig)
        {
            List<string> thumbnailUrls = new List<string>();

            // TODO new comment
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConfig.ConnectionString);

            // Create blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the container
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            BlobContinuationToken continuationToken = null;

            BlobResultSegment resultSegment = null;

            //Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
            //When the continuation token is null, the last page has been returned and execution can exit the loop.
            do
            {
                //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                //or by calling a different overload.
                resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 10, continuationToken, null, null);

                foreach (var blobItem in resultSegment.Results)
                {
                    thumbnailUrls.Add(blobItem.StorageUri.PrimaryUri.ToString());
                }

                //Get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            } while (continuationToken != null);

            return await Task.FromResult(thumbnailUrls);
        }
    }
}