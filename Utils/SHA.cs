using System;
using System.Security.Cryptography;
using System.Text;

namespace DMServer.Utils
{
    internal static class SHA
    {
        public static string SHA512Hash(string value)
        {
            byte[] encryptedBytes;

            using (var hashTool = SHA512.Create())
            {
                encryptedBytes = hashTool.ComputeHash(Encoding.UTF8.GetBytes(value));
            }

            return Convert.ToBase64String(encryptedBytes);
        }
    }
}
