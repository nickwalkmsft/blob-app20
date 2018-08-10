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
        private readonly AzureStorageConfig storageConfig;

        public BlobStorage(IOptions<AzureStorageConfig> storageConfig)
        {
            this.storageConfig = storageConfig.Value;
        }

        public Task Initialize()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.FileContainerName);
            return container.CreateIfNotExistsAsync();
        }

        public Task Save(Stream fileStream, string fileName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.FileContainerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            return blockBlob.UploadFromStreamAsync(fileStream);
            // TODO filename as metadata, store with a guid
        }

        // TODO Rename to something more appropriate and return pairs of (url, filename)
        public async Task<IEnumerable<string>> GetNames()
        {
            List<string> names = new List<string>();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.FileContainerName);

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

                names.AddRange(resultSegment.Results.OfType<ICloudBlob>().Select(b => b.Name));

                continuationToken = resultSegment.ContinuationToken;
            } while (continuationToken != null);

            return names;
        }

        // Use "id" instead of "name" (same for controller)
        public Task<Stream> Load(string filename)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConfig.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.FileContainerName);

            return container.GetBlobReference(filename).OpenReadAsync();
        }
    }
}