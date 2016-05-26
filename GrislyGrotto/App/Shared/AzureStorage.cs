using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace GrislyGrotto.App.Shared
{
    public static class AzureStorage
    {
        private static async Task<CloudBlobContainer> GetContainer(string connectionStringName = "GrislyGrottoAzureStorage", string containerName = "usercontentpub")
        {
            var accessKey = WebConfigurationManager.ConnectionStrings[connectionStringName].ToString();
            var account = CloudStorageAccount.Parse(accessKey);

            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            return container;
        }

        public static async Task<Tuple<bool, string>> Upload(string fileName, Stream stream)
        {
            var container = await GetContainer();
            var blob = container.GetBlockBlobReference(fileName);

            if (await blob.ExistsAsync())
                return Tuple.Create(false, (string)null);

            blob.Properties.ContentType = MimeMapping.GetMimeMapping(fileName);
            await blob.UploadFromStreamAsync(stream);

            return Tuple.Create(true, blob.Uri.ToString());
        }
    }
}