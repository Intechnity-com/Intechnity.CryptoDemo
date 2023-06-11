using System.Security.Cryptography;
using System.Text;

namespace Intechnity.CryptoDemo.Core.Tools;

public static class HashTools
{
    public static byte[] Sha256Bytes(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(nameof(value));

        using (var hash = SHA256.Create())
        {
            Encoding enc = Encoding.UTF8;
            byte[] result = hash.ComputeHash(enc.GetBytes(value));

            return result;
        }
    }

    public static string Sha256Hash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(nameof(value));

        var bytes = Sha256Bytes(value);

        var sb = new StringBuilder();
        foreach (byte b in bytes)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}