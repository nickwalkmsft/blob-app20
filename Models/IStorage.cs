using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileUploader.Models
{
    public interface IStorage
    {
        Task Save(Stream fileStream, string fileName);
        Task<IEnumerable<string>> GetFileUrls(string baseUrl);
        Task<Stream> GetFile(string filename);
    }
}