
using Microsoft.AspNetCore.Http;
using OceansApp.Models.ViewModels.Blobs;
using OceansApp.Models.ViewModels.Components;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IAzureBlobRepository
    {
        Task<List<BlobUploadResult>> UploadFilesAsync(string containerName, List<IFormFile> files, int elementId);
        Task DownloadFileAsync(string containerName, string fileName, string downloadPath);
        Task ListBlobsAsync(string containerName);
        Task<MethodResponse> DeleteBlobAsync(string containerName, string fileName);
    }
}
