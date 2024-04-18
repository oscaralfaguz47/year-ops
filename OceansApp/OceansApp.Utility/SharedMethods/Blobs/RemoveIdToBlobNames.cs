
using System.Text.RegularExpressions;

namespace OceansApp.Utility.SharedMethods.Blobs
{
    public static class RemoveIdToBlobNames
    {
        public static string RemoveId(string entireBlobName)
        {
            string pattern = @"^[a-f0-9]{32}_\d+_";
            return Regex.Replace(entireBlobName, pattern, "");
        }
    }
}
