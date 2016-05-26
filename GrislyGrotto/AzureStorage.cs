using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace GrislyGrotto
{
    public static class AzureStorage
    {
        private static CloudStorageAccount GetAccount(string connectionStringName = "GrislyGrottoAzureStorage")
        {
            var accessKey = WebConfigurationManager.ConnectionStrings[connectionStringName].ToString();
            return CloudStorageAccount.Parse(accessKey);
        }

        public static async Task<bool> Exists(string fileName, string containerName)
        {
            var account = GetAccount();
            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference(containerName);
            if (!(await container.ExistsAsync()))
                return false;

            var blob = container.GetBlockBlobReference(fileName);
            return await blob.ExistsAsync();
        }

        public static async Task<string> Upload(string fileName, string containerName, Stream stream)
        {
            var account = GetAccount();
            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            var newBlob = container.GetBlockBlobReference(fileName);
            newBlob.Properties.ContentType = MimeMapping.GetMimeMapping(fileName);
            await newBlob.UploadFromStreamAsync(stream);

            return newBlob.Uri.ToString();
        }
    }
}