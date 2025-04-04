using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Helpers
{
    public class Common
    {
        public static async Task<string> SaveDocument(string base64, string? name, string subContainerName, IConfiguration _configuration, string companyCode, ExtractTokenDto token)
        {
            string profileImage = "";
            try
            {
                var image = base64.Split(",");
                var content = image[0].Split(";");
                var contentResult = content[0].Split(":");
                string ContentType = contentResult[1];

                base64 = image[1];

                // Map ContentType to a file extension
                string fileExtension = ContentType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/gif" => ".gif",
                    "application/pdf" => ".pdf",
                    "text/plain" => ".txt",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                    "application/msword" => ".doc",
                    _ => "" // Default to no extension if ContentType is unrecognized
                };

                string imageName = Guid.NewGuid().ToString() + (string.IsNullOrEmpty(name) ? null : "_" + name) + fileExtension;
                string azureConnection = _configuration["AzureBlob:AzureBlobStorage"].ToString();
                string containerName = _configuration["AzureBlob:Container"].ToString();

                var container = new BlobContainerClient(azureConnection, containerName);
                var createResponse = await container.CreateIfNotExistsAsync();

                if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                    await container.SetAccessPolicyAsync(PublicAccessType.Blob);

                // Construct the blob path with folders
                string blobPath = string.IsNullOrEmpty(subContainerName) ? $"{companyCode}/{imageName}" : $"{companyCode}/{subContainerName}/{imageName}";
                var blob = container.GetBlobClient(blobPath);

                await blob.DeleteIfExistsAsync((DeleteSnapshotsOption)DeleteSnapshotsOption.IncludeSnapshots);

                var bytes = Convert.FromBase64String(base64);
                using (var fileStream = new MemoryStream(bytes))
                {
                    await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = ContentType });
                }

                if (!string.IsNullOrEmpty(blob.Uri.ToString()))
                {
                    profileImage = blob.Uri.ToString();
                }
            }
            catch (Exception ex)
            {
                profileImage = ex.Message;
            }
            return profileImage;
        }
    }
}
