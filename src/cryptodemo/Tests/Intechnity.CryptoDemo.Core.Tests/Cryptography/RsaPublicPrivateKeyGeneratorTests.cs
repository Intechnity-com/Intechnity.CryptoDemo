using FluentAssertions;
using Xunit;
using Intechnity.CryptoDemo.Core.Cryptography;
using Intechnity.CryptoDemo.Tests.Common.Fixtures;

namespace Intechnity.CryptoDemo.Core.Tests.Cryptography;

public class RsaPublicPrivateKeyGeneratorTests
{
    private const string TestDataToSign = "1234567890qwerty";
    private const string TestDataSignature = @"40A8C1DF7F0AFD7974AF6455F2B8175550A8D94047D081ED752ECBEF2E036137BBCB36948C41FE3FE9D55B41A3D642094966591D00C53AD180F54C69917E062B2498FEE599C84C4FD422F295FC5A582C093658EF1897EBC6F77DEDE2EC1B19F8E0486F3A061531C05C5F4082157670BB3303BD291A8D0F4D7A31B8F9F04E6A90";

    /// <summary>
    /// Private key: 3082025D02010002818100BB260250230D5F585D47D8604897A642FD8FED9EEFF1E7D6466ADD94AABBED88883ED03429BECBBCD5D721CFF4C655D4B436DF6EE2940B03AEB8757591DDAA72B57F70D5CE144B93325864EF877C56C2E2020AF1BD68B29AF8FDB467DFFCE106609A1FF04BFB1B74DB8F5FA467A39FDF54D94D2C9768089AE692CE1F5C93BBBD020301000102818024465EC667B4ECD934E37B26568BC6774FACB93348710C2DF5623B66D89D2A927F4E258F26D44BAB638B9A8B3377CA253B753363CBD9443547858B86E96A1CE25F9D97CA27B1A73FC5BE9DC591A389ED83B194DD46BDD8F9755D45D612138EAE487692AE891060F1DE684AAEDAAE32E3BD00CB20F8824957771256D9D21322E5024100F7445F55CF120C98528F60DEB9D1B65C5A1DEBCF3C0373ACDE68932D6F0D1A0E125C258EC914E825624065B458410FBF2A51292EC69CFC5141A394CE42910107024100C1C2155AAF06C8120275D0A6FE4AF868079DFA99CC1E8920514D76727CD5903D0604850CB1931C4FAB151DB4B3F8A0F97EC7942567209443592A07F5A66A601B0241008E64053278E83FF087BAA1622147AEE847CCEB3A8FA8BD38536B8D35A0B9BEC353B754B980FA5525120B5B861B3C7C40EDB18731B4963A86E071BF037839E089024100AEA7475E75F4F3F59FF8193BDE6F2FD97E3C3E3FD24A182B647E21EDF36F02D4AF0EF5EBCA49A434802FC99630C046427FB0616F11B5C14FB018FE11C58F8FCF024007FBC933E7C74FAD5A8D0A2583D828E5F908FD6C2733407D4A86E533E026860EDD598C2D2B7143380B68E3074122EBF99AD6D39E595C96F4ED78784E38437067
    /// </summary>
    private const string TestPublicKey = @"30818902818100BB260250230D5F585D47D8604897A642FD8FED9EEFF1E7D6466ADD94AABBED88883ED03429BECBBCD5D721CFF4C655D4B436DF6EE2940B03AEB8757591DDAA72B57F70D5CE144B93325864EF877C56C2E2020AF1BD68B29AF8FDB467DFFCE106609A1FF04BFB1B74DB8F5FA467A39FDF54D94D2C9768089AE692CE1F5C93BBBD0203010001";

    private readonly RsaPublicPrivateKeyGenerator _keyGenerator;

    public RsaPublicPrivateKeyGeneratorTests()
    {
        var dataProtector = new DataProtectorFixture()
            .SetupProtectToReturnInput()
            .SetupUnprotectToReturnInput()
            .Create();

        _keyGenerator = new RsaPublicPrivateKeyGenerator(dataProtector);
    }

    [Fact]
    public void GeneratePublicPrivateKeyPair_GeneratesValidData()
    {
        var (publicKey, _) = _keyGenerator.GeneratePublicPrivateKeyPair();

        publicKey.Should().HaveLength(280); // the RSA public key must be this long
    }

    [Fact]
    public void VerifySignature_ValidSignature_RandomData_ValidatesSuccessfully()
    {
        var (publicKey, protectedPrivateKey) = _keyGenerator.GeneratePublicPrivateKeyPair();

        var signature = _keyGenerator.SignWithPrivateKey(TestDataToSign, protectedPrivateKey);

        _keyGenerator.VerifySignature(TestDataToSign, signature, publicKey).Should().BeTrue();
    }

    [Fact]
    public void VerifySignature_ValidSignature_PregeneratedData_ValidatesSuccessfully()
    {
        _keyGenerator.VerifySignature(TestDataToSign, TestDataSignature, TestPublicKey).Should().BeTrue();
    }

    [Fact]
    public void VerifySignature_InvalidSignature_RandomData_FailsValidation()
    {
        var (publicKey, protectedPrivateKey) = _keyGenerator.GeneratePublicPrivateKeyPair();

        var signature = _keyGenerator.SignWithPrivateKey(TestDataToSign, protectedPrivateKey);
        signature = $"00{signature}";

        _keyGenerator.VerifySignature(TestDataToSign, signature, publicKey).Should().BeFalse();
    }

    [Fact]
    public void VerifySignature_InvalidSignature_PregeneratedData_FailsValidation()
    {
        //replace 1 byte
        var modifiedTestDataSignature = $"41{TestDataSignature[2..]}";

        _keyGenerator.VerifySignature(TestDataToSign, modifiedTestDataSignature, TestPublicKey).Should().BeFalse();
    }
}