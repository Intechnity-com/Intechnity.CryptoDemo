namespace Intechnity.CryptoDemo.Core.Extensions;

public static class BytesArrayExtensions
{
    public static string BytesToHex(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }
}