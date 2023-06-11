namespace Intechnity.CryptoDemo.Core.Providers;

public class RealDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}