using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileUploader.Helpers
{
    public interface IStorageHelper
    {
        Task UploadFileToStorage(Stream fileStream, string fileName);
        Task<List<string>> GetFileUrls(string baseUrl);
        Task<Stream> GetFile(string name);
    }
}