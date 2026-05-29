using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;

Console.WriteLine("=== Connection String Encryptor ===");
Console.WriteLine();

// New connection string with updated credentials
string newConnectionString = "Server=10.41.17.2\\WINWIN_SQL;Initial Catalog=Balmy_Agile;Persist Security Info=True;User ID=CustomerApp_Login;Password=Nrx@2026Sql!App77;Encrypt=True;TrustServerCertificate=True;";

// New Hangfire connection string
string newHangfireConnection = "Server=10.41.17.2\\WINWIN_SQL;Initial Catalog=Hangfire;Persist Security Info=True;User ID=CustomerApp_Login;Password=Nrx@2026Sql!App77;Encrypt=True;TrustServerCertificate=True;";

// AES Keys from appsettings.json
string keyBase64 = "seXWRTqvXyO+0iLMdaQHKHMNgct2UjXehUPzoS8I8Hg=";
string ivBase64 = "NSFpnL9GP0lqwjmk7j1PrQ==";

try
{
    byte[] key = Convert.FromBase64String(keyBase64);
    byte[] iv = Convert.FromBase64String(ivBase64);

    Console.WriteLine("Original DatabaseConnection String:");
    Console.WriteLine(newConnectionString);
    Console.WriteLine();

    string encryptedDatabase = EncryptString(newConnectionString, key, iv);
    
    Console.WriteLine("[SUCCESS] Encrypted DatabaseConnection:");
    Console.WriteLine(encryptedDatabase);
    Console.WriteLine();
    Console.WriteLine("---");
    Console.WriteLine();

    Console.WriteLine("Original HangfireConnection String:");
    Console.WriteLine(newHangfireConnection);
    Console.WriteLine();

    Console.WriteLine("[PLAINTEXT] HangfireConnection (not encrypted):");
    Console.WriteLine(newHangfireConnection);
}
catch (Exception ex)
{
    Console.WriteLine($"[ERROR] {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

string EncryptString(string plainText, byte[] key, byte[] iv)
{
    using (Aes aes = Aes.Create())
    {
        aes.Key = key;
        aes.IV = iv;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (StreamWriter sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}

