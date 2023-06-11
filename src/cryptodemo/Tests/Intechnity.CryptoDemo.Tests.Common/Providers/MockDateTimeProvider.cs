using Intechnity.CryptoDemo.Core.Providers;

namespace Intechnity.CryptoDemo.Tests.Common.Providers;

public class MockDateTimeProvider : IDateTimeProvider
{
    private DateTimeOffset? _dateTimeOffset;

    public DateTimeOffset Now => _dateTimeOffset ?? DateTimeOffset.Now;

    public void SetReturnDate(DateTimeOffset? date)
    {
        _dateTimeOffset = date;
    }
}