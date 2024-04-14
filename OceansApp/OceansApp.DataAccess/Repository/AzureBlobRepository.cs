using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.DataAccess.Repository.IRepository;

namespace OceansApp.DataAccess.Repository
{
    public class AzureBlobRepository: IAzureBlobRepository
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _config;

        public AzureBlobRepository(IConfiguration config)
        {
            _config = config;
            _blobServiceClient = new BlobServiceClient(Environment.GetEnvironmentVariable(_config["AzureBlobStorage:ConnectionString"]));
        }
        private async Task<string> CalculateContentHashAsync(IFormFile file)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = file.OpenReadStream())
                {
                    var hashBytes = await md5.ComputeHashAsync(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public async Task<List<BlobUploadResult>> UploadFilesAsync(string containerName, List<IFormFile> files)
        {
            var uploadResults = new List<BlobUploadResult>();
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            foreach (var file in files)
            {
                var blobClient = containerClient.GetBlobClient(file.FileName);
                var existingBlob = await blobClient.ExistsAsync();

                var uploadResult = new BlobUploadResult
                {
                    FileName = file.FileName,
                    Size = file.Length,
                    ContentType = file.ContentType ?? "application/octet-stream", // Ensure a default ContentType
                    UploadDate = DateTime.UtcNow,
                    Success = true  // Assume success, change based on blob existence
                };

                if (existingBlob)
                {
                    uploadResult.Success = false;
                    uploadResult.ErrorMessage = "File already exists and has not been modified.";
                    uploadResults.Add(uploadResult); // Add the result for existing files here
                    continue; // Skip the upload process for this file
                }

                // Process for new or changed files
                try
                {
                    string contentHash = await CalculateContentHashAsync(file);
                    string uniqueFilename = $"{contentHash}_{file.FileName}";
                    blobClient = containerClient.GetBlobClient(uniqueFilename); // Reassign to the unique path

                    using (var stream = file.OpenReadStream())
                    {
                        var blobHttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType };
                        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                        uploadResult.FileName = uniqueFilename;
                        uploadResult.BlobUrl = blobClient.Uri.ToString();
                        uploadResult.Success = true; // Confirm success after successful upload
                    }
                }
                catch (Exception ex)
                {
                    uploadResult.Success = false;
                    uploadResult.ErrorMessage = $"Error uploading file: {ex.Message}";
                }

                uploadResults.Add(uploadResult); // Add result only if file is new or changed and processed
            }

            return uploadResults;
        }







        public async Task DownloadFileAsync(string containerName, string fileName, string downloadPath)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            using (var fileStream = File.OpenWrite(downloadPath))
            {
                await download.Content.CopyToAsync(fileStream);
                fileStream.Close();
            }
            Console.WriteLine($"File downloaded to {downloadPath}");
        }

        public async Task ListBlobsAsync(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine(blobItem.Name);
            }
        }

        public async Task DeleteBlobAsync(string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
            Console.WriteLine($"Blob {fileName} deleted.");
        }
    }
}
