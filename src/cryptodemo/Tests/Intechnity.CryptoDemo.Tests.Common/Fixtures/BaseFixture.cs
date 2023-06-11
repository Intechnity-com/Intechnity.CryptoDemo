using Moq;

namespace Intechnity.CryptoDemo.Tests.Common.Fixtures;

public abstract class BaseFixture<T> where T : class
{
    public Mock<T> MockedService;

    protected BaseFixture(bool callBase = false)
    {
        MockedService = new Mock<T> { CallBase = callBase };
    }

    public virtual T Create()
    {
        return MockedService.Object;
    }
}