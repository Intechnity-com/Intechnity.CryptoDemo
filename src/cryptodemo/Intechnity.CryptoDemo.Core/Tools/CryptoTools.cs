using System.Security.Cryptography;

namespace Intechnity.CryptoDemo.Core.Tools;

public class CryptoTools
{
    private const int SALT_SIZE = 8;

    public static void Encrypt(byte[] data, string targetFile, string password)
    {
        var keyGenerator = new Rfc2898DeriveBytes(password, SALT_SIZE);
        var aes = Aes.Create();

        aes.IV = keyGenerator.GetBytes(aes.BlockSize / 8);
        aes.Key = keyGenerator.GetBytes(aes.KeySize / 8);

        var fileInfo = new FileInfo(targetFile);
        using (var fileStream = fileInfo.Create())
        {
            fileStream.Write(keyGenerator.Salt, 0, SALT_SIZE);

            using (var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
            }
        }
    }

    public static byte[] Decrypt(string sourceFile, string password)
    {
        var fileInfo = new FileInfo(sourceFile);

        var fileStream = fileInfo.OpenRead();
        var salt = new byte[SALT_SIZE];
        fileStream.Read(salt, 0, SALT_SIZE);

        var keyGenerator = new Rfc2898DeriveBytes(password, salt);
        var aes = Aes.Create();
        aes.IV = keyGenerator.GetBytes(aes.BlockSize / 8);
        aes.Key = keyGenerator.GetBytes(aes.KeySize / 8);

        using (var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
        {
            using (var output = new MemoryStream())
            {
                cryptoStream.CopyTo(output);
                return output.ToArray();
            }
        }
    }
}