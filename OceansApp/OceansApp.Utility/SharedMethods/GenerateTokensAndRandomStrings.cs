
using System.Security.Cryptography;
using System.Text;

namespace OceansApp.Utility.SharedMethods
{
    public class GenerateTokensAndRandomStrings
    {
        private const string LowerCaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string UpperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string NumericChars = "0123456789";
        private const string SpecialChars = "!@#$%^&*";
        private const int PasswordLength = 10;
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

        public static string GeneratePassword()
        {
            StringBuilder password = new StringBuilder();
            Random random = new Random();

            // Ensure at least one character of each type
            password.Append(LowerCaseChars[random.Next(LowerCaseChars.Length)]);
            password.Append(UpperCaseChars[random.Next(UpperCaseChars.Length)]);
            password.Append(NumericChars[random.Next(NumericChars.Length)]);
            password.Append(SpecialChars[random.Next(SpecialChars.Length)]);

            // Fill password length with a mix of all types
            string allChars = LowerCaseChars + UpperCaseChars + NumericChars + SpecialChars;
            for (int i = password.Length; i < PasswordLength; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Optional: Mix the password so that the order of the character types is not predictable
            return Shuffle(password.ToString());
        }

        private static string Shuffle(string input)
        {
            var array = input.ToCharArray();
            Random rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = array[k];
                array[k] = array[n];
                array[n] = value;
            }

            return new string(array);
        }


    }
}
