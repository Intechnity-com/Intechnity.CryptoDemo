namespace Intechnity.CryptoDemo.Core.Cryptography;

public interface IPublicPrivateKeyGenerator
{
    PublicPrivateKeyPair GeneratePublicPrivateKeyPair();

    PublicPrivateKeyPair GetPublicPrivateKeyPair(string publicKeyHex, string privateKeyHex);

    string SignWithPrivateKey(string data, byte[] protectedPrivateKeyBytes);

    bool VerifySignature(string data, string signature, string publicKey);
}