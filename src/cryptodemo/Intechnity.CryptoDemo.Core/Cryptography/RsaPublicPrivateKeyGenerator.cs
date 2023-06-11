using System.Security.Cryptography;
using System.Text;
using Intechnity.CryptoDemo.Core.Extensions;

namespace Intechnity.CryptoDemo.Core.Cryptography;

public class RsaPublicPrivateKeyGenerator : IPublicPrivateKeyGenerator
{
    public const int KEY_SIZE = 1024;

    private readonly IDataProtector _dataProtector;

    public RsaPublicPrivateKeyGenerator(IDataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public PublicPrivateKeyPair GeneratePublicPrivateKeyPair()
    {
        var rsa = RSA.Create(KEY_SIZE);

        var publicKey = rsa.ExportRSAPublicKey();
        var publicKeyHex = publicKey.BytesToHex();

        var privateKey = rsa.ExportRSAPrivateKey();
        var protectedPrivateKey = _dataProtector.Protect(privateKey);

        return new PublicPrivateKeyPair(publicKeyHex, protectedPrivateKey);
    }

    public PublicPrivateKeyPair GetPublicPrivateKeyPair(string publicKeyHex, string privateKeyHex)
    {
        var privateKeyBytes = privateKeyHex.HexToBytes();
        var protectedPrivateKey = _dataProtector.Protect(privateKeyBytes);

        return new PublicPrivateKeyPair(publicKeyHex, protectedPrivateKey);
    }

    public string SignWithPrivateKey(string data, byte[] protectedPrivateKeyBytes)
    {
        var rsa = RSA.Create(KEY_SIZE);

        var unprotectedPrivateKeyBytes = _dataProtector.Unprotect(protectedPrivateKeyBytes);
        rsa.ImportRSAPrivateKey(unprotectedPrivateKeyBytes, out int _);

        var dataBytes = Encoding.UTF8.GetBytes(data);
        var signed = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var signedHex = signed.BytesToHex();

        return signedHex;
    }

    public bool VerifySignature(string data, string signature, string publicKey)
    {
        var rsa = RSA.Create(KEY_SIZE);

        var publicKeyBytes = publicKey.HexToBytes();
        rsa.ImportRSAPublicKey(publicKeyBytes, out int _);

        var dataBytes = Encoding.UTF8.GetBytes(data);

        var signatureBytes = signature.HexToBytes();

        return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}