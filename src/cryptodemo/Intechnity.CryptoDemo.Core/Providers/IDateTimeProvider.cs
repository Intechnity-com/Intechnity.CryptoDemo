namespace Intechnity.CryptoDemo.Core.Providers;

public interface IDateTimeProvider
{
    DateTimeOffset Now { get; }
}