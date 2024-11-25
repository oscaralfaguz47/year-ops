using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.Utility.SharedMethods.Blobs;
using Azure;
using Azure.Storage.Sas;
using Azure.Storage;
using OceansApp.Models.ViewModels.Components;
using System.Text.RegularExpressions;

namespace OceansApp.DataAccess.Repository
{
    public class AzureBlobRepository: IAzureBlobRepository
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _accountKey;
        private readonly IConfiguration _config;

        public AzureBlobRepository(IConfiguration config)
        {
            _config = config;
            _blobServiceClient = new BlobServiceClient(_config["FilesStorageAccountENV"]);
            _accountKey = _config["FileStorageAccountKeyENV"];
        }

        public async Task<List<BlobUploadResult>> UploadFilesAsync(string containerId, List<IFormFile> files, string? elementId, int validDays)
        {
            var uploadResults = new List<BlobUploadResult>();
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerId);

            foreach (var file in files)
            {
                CalculateContentHash calculateHash = new CalculateContentHash();
                string contentHash = await calculateHash.CalculateContentHashAsync(file);
                string normalizedFileName = NormalizeFileName(file.FileName);
                string uniqueFilename = $"{contentHash}{(elementId == null ? "": "_" + elementId)}_{normalizedFileName}";
                var blobClient = containerClient.GetBlobClient(uniqueFilename);

                var uploadResult = new BlobUploadResult
                {
                    FileName = uniqueFilename,
                    Size = file.Length,
                    ContentType = file.ContentType ?? "application/octet-stream", // Ensure a default ContentType
                    ContainerId = containerId,
                    UploadDate = DateTime.UtcNow,
                    Success = true  // Assume success initially
                };

                // Upload process
                try
                {
                    using (var stream = file.OpenReadStream())
                    {
                        var blobHttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType };
                        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders, 
                            Conditions = new BlobRequestConditions { IfNoneMatch = new ETag("*") } });

                        // Generate SAS for the blob
                        string sasUrl = GenerateBlobSasUri(containerClient, uniqueFilename, validDays);
                        uploadResult.BlobUrl = sasUrl;
                        uploadResult.Success = true; // Confirm success after a successful upload
                    }
                }
                catch (Exception ex)
                {
                    uploadResult.Success = false;
                    uploadResult.ErrorMessage = $"Error uploading file: {ex.Message}";
                }

                uploadResults.Add(uploadResult); // Add the result of the process
            }
            return uploadResults;
        }

        private string NormalizeFileName(string fileName)
        {
            // Get the file extension (e.g., ".png")
            var fileExtension = Path.GetExtension(fileName);

            // Get the file name without extension
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            // Replace invalid characters with underscores
            fileNameWithoutExtension = Regex.Replace(fileNameWithoutExtension, @"[^a-zA-Z0-9_\-]", "_");

            // Limit the file name length to avoid overly long names (e.g., 100 characters max)
            if (fileNameWithoutExtension.Length > 100)
            {
                fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, 100);
            }

            // Rebuild the file name with the sanitized name and original extension
            return $"{fileNameWithoutExtension}{fileExtension}";
        }


        public string GenerateBlobSasUri(BlobContainerClient containerClient, string blobName, int validDays, string storedPolicyName = null)
        {
            var blobClient = containerClient.GetBlobClient(blobName);

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(validDays) // Token valid for days
            };

            if (storedPolicyName == null)
            {
                sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            BlobUriBuilder blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
            {
                Sas = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_blobServiceClient.AccountName, _accountKey))
            };

            return blobUriBuilder.ToUri().ToString();
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

        public async Task<MethodResponse> DeleteBlobAsync(string containerName, string fileName)
        {
            MethodResponse response = new MethodResponse();
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                Response<bool> deleteBlobResponse = await blobClient.DeleteIfExistsAsync();

                if (deleteBlobResponse.Value)
                {
                    response.Success = true;
                    response.Message = $"The file ({RemoveIdToBlobNames.RemoveId(fileName)}) was deleted!";
                }
                else
                {
                    response.Success = false;
                    response.Message = $"File does not exist: {fileName}";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting file: {ex.Message}";
            }

            return response;
        }

    }
}
