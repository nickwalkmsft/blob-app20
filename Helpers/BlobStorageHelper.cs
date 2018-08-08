using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using FileUploader.Models;

namespace FileUploader.Helpers
{
    public class BlobStorageHelper : IStorageHelper
    {
        private readonly string containerName;
        private readonly CloudStorageAccount storageAccount;

        public BlobStorageHelper(IOptions<AzureStorageConfig> storageConfig)
        {
            this.storageAccount = CloudStorageAccount.Parse(storageConfig.Value.ConnectionString);
            this.containerName = storageConfig.Value.FileContainer;
        }

        public Task UploadFileToStorage(Stream fileStream, string fileName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            return blockBlob.UploadFromStreamAsync(fileStream);
        }

        public async Task<List<string>> GetFileUrls(string baseUrl)
        {
            List<string> fileUrls = new List<string>();

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;

            do
            {
                // TODO use a simpler overload

                //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                //or by calling a different overload.
                resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 10, continuationToken, null, null);

                // fileUrls.AddRange(resultSegment.Results.Select(r => r.StorageUri.PrimaryUri.ToString()));

                fileUrls.AddRange(resultSegment.Results.Select(r => $"{baseUrl}/{r.Uri.Segments.Last()}"));

                //Get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            } while (continuationToken != null);

            return await Task.FromResult(fileUrls);
        }

        public Task<Stream> GetFile(string name)
        {
            // Create blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the container
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            return container.GetBlobReference(name).OpenReadAsync();
        }
    }
}