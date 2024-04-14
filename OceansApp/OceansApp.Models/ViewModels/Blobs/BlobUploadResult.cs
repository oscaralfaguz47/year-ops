
namespace OceansApp.Models.ViewModels.Blobs
{
    public class BlobUploadResult
    {
        public string FileName { get; set; }
        public string BlobUrl { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadDate { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
