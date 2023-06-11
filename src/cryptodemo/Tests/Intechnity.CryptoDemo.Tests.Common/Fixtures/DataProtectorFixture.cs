using Moq;
using Intechnity.CryptoDemo.Core.Cryptography;

namespace Intechnity.CryptoDemo.Tests.Common.Fixtures;

public class DataProtectorFixture : BaseFixture<IDataProtector>
{
    public DataProtectorFixture SetupProtectToReturnInput()
    {
        MockedService.Setup(x => x.Protect(It.IsAny<byte[]>())).Returns((byte[] input) => input);

        return this;
    }

    public DataProtectorFixture SetupUnprotectToReturnInput()
    {
        MockedService.Setup(x => x.Unprotect(It.IsAny<byte[]>())).Returns((byte[] input) => input);

        return this;
    }
}