namespace VRFace.IO
{
    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.File;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;

    public class FileStorage
    {
        public static async Task<string> uploadImage(string data, string fileName)
        {
            // Retrieve storage account information from connection string
            // How to create a storage connection string - http://msdn.microsoft.com/en-us/library/azure/ee758697.aspx
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Create a file client for interacting with the file service.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            CloudFileShare share = fileClient.GetShareReference(ConfigurationManager.AppSettings["FileShare"]);

            try
            {
                await share.CreateIfNotExistsAsync();
            }
            catch (StorageException)
            {
                throw;
            }

            // Get a reference to the root directory of the share.        
            CloudFileDirectory root = share.GetRootDirectoryReference();

            // Create a directory under the root directory 
            CloudFileDirectory dir = root.GetDirectoryReference(Guid.NewGuid().ToString());
            await dir.CreateIfNotExistsAsync();

            CloudFile file = dir.GetFileReference(data);
            await file.UploadFromFileAsync(fileName);

            return file.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Validates the connection string information in app.config and throws an exception if it looks like 
        /// the user hasn't updated this to valid values. 
        /// </summary>
        /// <param name="storageConnectionString">The storage connection string</param>
        /// <returns>CloudStorageAccount object</returns>
        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }

            return storageAccount;
        }
    }
}