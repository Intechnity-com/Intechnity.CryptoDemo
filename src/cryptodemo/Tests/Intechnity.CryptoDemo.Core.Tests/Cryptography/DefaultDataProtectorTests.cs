using FluentAssertions;
using System.Text;
using Xunit;
using Intechnity.CryptoDemo.Core.Cryptography;

namespace Intechnity.CryptoDemo.Core.Tests.Cryptography;

public class DefaultDataProtectorTests
{
    private readonly DefaultDataProtector _defaultDataProtector;

    public DefaultDataProtectorTests()
    {
        _defaultDataProtector = new DefaultDataProtector();
    }

    [Fact]
    public void Protect_ProtectingData_BytesAreDifferent()
    {
        var testData = "confidential string";
        var originalBytes = Encoding.UTF8.GetBytes(testData);

        var protectedBytes = _defaultDataProtector.Protect(originalBytes);
        var unprotectedBytes = _defaultDataProtector.Unprotect(protectedBytes);

        protectedBytes.Should().NotBeEquivalentTo(unprotectedBytes);
    }

    [Fact]
    public void Unprotect_UnrpotectingProtectedData_DataIsProperlyConverted()
    {
        var testData = "confidential string";
        var originalBytes = Encoding.UTF8.GetBytes(testData);

        var protectedBytes = _defaultDataProtector.Protect(originalBytes);
        var unprotectedBytes = _defaultDataProtector.Unprotect(protectedBytes);

        unprotectedBytes.Should().BeEquivalentTo(originalBytes);
    }
}