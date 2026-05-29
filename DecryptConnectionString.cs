using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

Console.WriteLine("=== Connection String Decryptor ===");
Console.WriteLine();

// Encrypted connection string from appsettings.json
string encryptedString = "4vsT2JA+hBiPNP/igIFrdL6y+O4XMC5QSOut5cITAEYjbphGqJm0IzO3ROcRTWSMCYIo7g96Wnz1XJFzmmJXcHfsjDvL/vBMOLS9qglYxdOVPgnbtuXsauor66PuMejfGvEpDKmVygCYaF1BVr5V3XoYiFI9c/NNWWDwzo2DkaworN7x3YU0ZK69dSdqpWw937YgfvRtpfE0CCH1GwV34Q==";

// AES Keys from appsettings.json
string keyBase64 = "seXWRTqvXyO+0iLMdaQHKHMNgct2UjXehUPzoS8I8Hg=";
string ivBase64 = "NSFpnL9GP0lqwjmk7j1PrQ==";

try
{
    byte[] key = Convert.FromBase64String(keyBase64);
    byte[] iv = Convert.FromBase64String(ivBase64);
    byte[] encryptedData = Convert.FromBase64String(encryptedString);

    string decrypted = DecryptString(encryptedData, key, iv);
    
    Console.WriteLine("[SUCCESS] Decrypted Connection String:");
    Console.WriteLine(decrypted);
    Console.WriteLine();
    
    // Parse connection string
    ParseConnectionString(decrypted);
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] {ex.Message}");
}

string DecryptString(byte[] encryptedData, byte[] key, byte[] iv)
{
    using (Aes aes = Aes.Create())
    {
        aes.Key = key;
        aes.IV = iv;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream(encryptedData))
        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        using (StreamReader sr = new StreamReader(cs))
        {
            return sr.ReadToEnd();
        }
    }
}

void ParseConnectionString(string connectionString)
{
    Console.WriteLine("=== Parsed Components ===");
    
    string[] parts = connectionString.Split(';');
    foreach (string part in parts)
    {
        if (!string.IsNullOrWhiteSpace(part))
        {
            Console.WriteLine($"  {part}");
        }
    }
}
