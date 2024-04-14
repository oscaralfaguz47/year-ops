
using Microsoft.AspNetCore.Http;
using OceansApp.Models.ViewModels.Blobs;

namespace OceansApp.DataAccess.Repository.IRepository
{
    public interface IAzureBlobRepository
    {
        Task<List<BlobUploadResult>> UploadFilesAsync(string containerName, List<IFormFile> files);
        Task DownloadFileAsync(string containerName, string fileName, string downloadPath);
        Task ListBlobsAsync(string containerName);
        Task DeleteBlobAsync(string containerName, string fileName);
    }
}
