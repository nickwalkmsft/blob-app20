using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FileUploader.Models
{
    public class BlobStorage : IStorage
    {
        private readonly string containerName;
        private readonly CloudStorageAccount storageAccount;

        public BlobStorage(IOptions<AzureStorageConfig> storageConfig)
        {
            this.storageAccount = CloudStorageAccount.Parse(storageConfig.Value.ConnectionString);
            this.containerName = storageConfig.Value.FileContainer;
        }

        public Task Initialize()
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container.CreateIfNotExistsAsync();
        }

        public Task Save(Stream fileStream, string fileName)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            return blockBlob.UploadFromStreamAsync(fileStream);
        }

        public async Task<IEnumerable<string>> GetFileUrls(string baseUrl)
        {
            List<string> fileUrls = new List<string>();

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;

            do
            {
                resultSegment = await container.ListBlobsSegmentedAsync(
                    prefix: "",
                    useFlatBlobListing: true,
                    blobListingDetails: BlobListingDetails.All,
                    maxResults: null,
                    currentToken: continuationToken,
                    options: null,
                    operationContext: null);

                fileUrls.AddRange(resultSegment.Results.Select(r => $"{baseUrl}/{r.Uri.Segments.Last()}"));

                continuationToken = resultSegment.ContinuationToken;
            } while (continuationToken != null);

            return fileUrls;
        }

        public Task<Stream> GetFile(string filename)
        {
            // Create blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the container
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            return container.GetBlobReference(filename).OpenReadAsync();
        }
    }
}