using Framework.Core.Interfaces.ReadFiles;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.ReadFiles
{
    public class AzureBlobReader : IFileReader
    {

        public Stream GetFile(string location, string name)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(location);

            // Retrieve reference to a blob named "photo1.jpg".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);

            // Save blob contents to a file.
            //using (var memoryStream = new MemoryStream())
            //{
            var memoryStream = new MemoryStream();
            blockBlob.DownloadToStream(memoryStream);
            return memoryStream;
            //}
        }
    }
}
