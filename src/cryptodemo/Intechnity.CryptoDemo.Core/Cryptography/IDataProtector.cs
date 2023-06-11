namespace Intechnity.CryptoDemo.Core.Cryptography;

public interface IDataProtector
{
    byte[] Protect(byte[] data);

    byte[] Unprotect(byte[] data);
}