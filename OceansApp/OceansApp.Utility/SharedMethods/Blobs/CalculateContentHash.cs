
using Microsoft.AspNetCore.Http;

namespace OceansApp.Utility.SharedMethods.Blobs
{
    public class CalculateContentHash
    {
        public async Task<string> CalculateContentHashAsync(IFormFile file)
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
    }
}
