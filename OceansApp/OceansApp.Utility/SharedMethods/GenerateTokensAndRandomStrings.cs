
using System.Security.Cryptography;
using System.Text;

namespace OceansApp.Utility.SharedMethods
{
    public class GenerateTokensAndRandomStrings
    {
        public string GenerateOpaqueToken()
        {
            string uuid = Guid.NewGuid().ToString();
            string rawToken = $"{uuid}:{DateTime.UtcNow.Ticks}";
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
                return Convert.ToHexString(hashedBytes);
            }
        }

    }
}
