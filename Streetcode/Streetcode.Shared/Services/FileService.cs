using System.Security.Cryptography;
using System.Text;

namespace Streetcode.Shared.Services;

public static class FileService
{
    public static string HashFunction(string createdFileName)
    {
        using (var hash = SHA256.Create())
        {
            Encoding enc = Encoding.UTF8;
            byte[] result = hash.ComputeHash(enc.GetBytes(createdFileName));
            return Convert.ToBase64String(result).Replace('/', '_');
        }
    }
    
    public static byte[] EncryptBytes(byte[] plainBytes, string keyCrypt)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(keyCrypt);
        byte[] iv = new byte[16];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = keyBytes;
            aes.IV = iv;
        
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            {
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                
                byte[] encryptedData = new byte[iv.Length + encryptedBytes.Length];
                Buffer.BlockCopy(iv, 0, encryptedData, 0, iv.Length);
                Buffer.BlockCopy(encryptedBytes, 0, encryptedData, iv.Length, encryptedBytes.Length);
            
                return encryptedData;
            }
        }
    }

    public static byte[] DecryptBytes(byte[] encryptedData, string keyCrypt)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(keyCrypt);

        byte[] iv = new byte[16];
        Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);

        byte[] decryptedBytes;
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.Key = keyBytes;
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor();
            decryptedBytes = decryptor.TransformFinalBlock(encryptedData, iv.Length, encryptedData.Length - iv.Length);
        }

        return decryptedBytes;
    }

    public static string PrepareFileStorageName(string name)
    {
        return $"{DateTime.Now}{name}"
            .Replace(" ", "_")
            .Replace(".", "_")
            .Replace(":", "_");
    }
}