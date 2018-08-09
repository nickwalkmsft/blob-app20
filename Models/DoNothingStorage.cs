using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploader.Models
{
    public class DoNothingStorage : IStorage
    {
        public Task Save(Stream fileStream, string fileName)
        {
            // Do nothing
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetFileUrls(string baseUrl)
        {
            // Return an empty list
            return Task.FromResult(Enumerable.Empty<string>());
        }

        public Task<Stream> GetFile(string filename)
        {
            throw new ArgumentException($"File {name} does not exist");
        }
    }
}