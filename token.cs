using System;
using System.Security.Cryptography;

namespace BankingAppBackend
{
    public class token
    {
        public static void GenerateKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] key = new byte[32]; // 256 bits / 8 bits per byte = 32 bytes
                rng.GetBytes(key);
                string base64Key = Convert.ToBase64String(key);
                Console.WriteLine("Generated Key (Base64): " + base64Key);
            }
        }
    }
}
