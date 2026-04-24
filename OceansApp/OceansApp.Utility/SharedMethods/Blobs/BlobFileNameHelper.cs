
using System.Text.RegularExpressions;

namespace OceansApp.Utility.SharedMethods.Blobs
{
    public class BlobFileNameHelper
    {
        public static string NormalizeFileName(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            fileNameWithoutExtension = Regex.Replace(fileNameWithoutExtension, @"[^a-zA-Z0-9_\-]", "_");
            if (fileNameWithoutExtension.Length > 100)
            {
                fileNameWithoutExtension = fileNameWithoutExtension.Substring(0, 100);
            }
            return $"{fileNameWithoutExtension}{fileExtension}";
        }
    }
}
