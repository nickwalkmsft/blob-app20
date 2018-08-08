using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blobapp20.Models
{
    public class AzureStorageConfig
    {
        public string ConnectionString { get; set; }
        public string FileContainer { get; set; }
    }
}