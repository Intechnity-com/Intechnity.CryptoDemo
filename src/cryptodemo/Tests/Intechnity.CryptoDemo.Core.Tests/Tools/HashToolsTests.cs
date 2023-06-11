using FluentAssertions;
using Xunit;
using Intechnity.CryptoDemo.Core.Tools;

namespace Intechnity.CryptoDemo.Core.Tests.Tools;

public class HashToolsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Sha256Hash_NullOrWhitespaceInput_ThrowsException(string input)
    {
        Action action = () => HashTools.Sha256Hash(input);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("test", "9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08")]
    [InlineData("1", "6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b")]
    [InlineData("2", "d4735e3a265e16eee03f59718b9b5d03019c07d8b6c51f90da3a666eec13ab35")]
    [InlineData("CryptoDemo", "b9a64ba1bd84aa0c2894d8615440fd1229da1e78de59daa2ec260796108a558f")]
    [InlineData("CryptoDemon", "94d7ec14950a1dfbcdcc9f35f11dd6900eadfa3ffe5a6d37d5bc0b9f3ea048e1")]
    public void Sha256Hash_NonNullInput_ProperlyCalculatesHash(string input, string expectedResult)
    {
        var resultHash = HashTools.Sha256Hash(input);
        Assert.Equal(expectedResult, resultHash);
    }
}