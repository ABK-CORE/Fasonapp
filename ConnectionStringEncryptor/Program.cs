using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Enigma;

namespace ConnectionStringEncryptor
{
    internal class Program
    {
        static void Main()
        {
            HazirAESAnahtarlari();

            ShowMenu();
            Console.Write("Seçiminiz: ");
            var secim = Console.ReadLine()?.Trim();

            switch (secim)
            {
                case "1":
                    IsleConnectionStringSifrele();
                    break;
                case "2":
                    Console.WriteLine("2. seçenek seçildi.");
                    break;
                case "3":
                    Console.WriteLine("3. seçenek seçildi.");
                    break;
                default:
                    Console.WriteLine("Geçersiz seçim.");
                    break;
            }
        }

        /// <summary>
        /// KeyCase.Instance.Key ve Vektor null ise; önce kullanıcıdan almaya çalışır,
        /// girilmezse otomatik Aes.Create() ile üretir ve KeyCase'e set eder.
        /// </summary>
        private static void HazirAESAnahtarlari()
        {
            if (KeyCase.Instance.Key == null || KeyCase.Instance.Vektor == null)
            {
                Console.Write("AES Key (Base64) girin (ENTER geçmek için): ");
                var aesKey = Console.ReadLine();
                Console.Write("AES IV  (Base64) girin (ENTER geçmek için): ");
                var aesIV = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(aesKey) || string.IsNullOrWhiteSpace(aesIV))
                {
                    using var aes = Aes.Create();
                    aesKey = Convert.ToBase64String(aes.Key);
                    aesIV = Convert.ToBase64String(aes.IV);
                    Console.WriteLine($"[Oluşturuldu] AES Key: {aesKey}");
                    Console.WriteLine($"[Oluşturuldu] AES IV:  {aesIV}");
                }

                KeyCase.Instance.SetAesKeys(
                    Convert.FromBase64String(aesKey),
                    Convert.FromBase64String(aesIV)
                );
            }
        }

        private static void ShowMenu()
        {
            Console.WriteLine("1. Yeni ConnectionString Gir ve Şifrelenmiş Halini Yazdır");
            Console.WriteLine("2. Seçenek 2");
            Console.WriteLine("3. Seçenek 3");
        }

        private static void IsleConnectionStringSifrele()
        {
            Console.Write("ConnectionString girin: ");
            var cs = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(cs))
            {
                Console.WriteLine("[X] Boş değer girilemez.");
                return;
            }

            if (!TestConnectionString(cs))
            {
                Console.WriteLine("[X] Bağlantı testi başarısız.");
                return;
            }

            var sifreli = EncryptConnectionString(cs);
            Console.WriteLine("\n[✓] Şifrelenmiş hal:");
            Console.WriteLine(sifreli);
        }

        private static bool TestConnectionString(string connectionString)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string EncryptConnectionString(string plainText)
        {
            using var aesAlg = Aes.Create();
            aesAlg.Key = KeyCase.Instance.Key;
            aesAlg.IV = KeyCase.Instance.Vektor;

            using var encryptor = aesAlg.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
