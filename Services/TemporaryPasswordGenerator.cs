using System.Security.Cryptography;
using System.Text;

namespace HRManagementSystem.Services
{
    public class TemporaryPasswordGenerator // This is a custom built service that generates secure random TEMPORARY passwords for the new user accounts
    {
        private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";

        public string GenerateRandomPassword(int length = 12)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);

            foreach (var b in bytes)
            {
                sb.Append(AllowedChars[b % AllowedChars.Length]);
            }

            return sb.Append("!1").ToString(); // It was added so the randomly generated password always contains an non-alphanumeric charachter (because that's a Identity req)
        }
    }
}
