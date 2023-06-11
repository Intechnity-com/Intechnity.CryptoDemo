namespace Intechnity.CryptoDemo.Core.Extensions;

public static class StringExtensions
{
    public static byte[] HexToBytes(this string hex)
    {
        return Enumerable.Range(0, hex.Length)
                 .Where(x => x % 2 == 0)
                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                 .ToArray();
    }

    public static string EmptyWhenNull(this string? input)
    {
        if (input == null)
            return string.Empty;

        return input;
    }
}