using System.Security.Cryptography;

namespace Intechnity.CryptoDemo.Core.Cryptography;

public class DefaultDataProtector : IDataProtector
{
    private static byte[] _additionalEntropy = { 9, 5, 3, 6, 1 };

    public byte[] Protect(byte[] data)
    {
        // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
        // only by the same current user.
        return ProtectedData.Protect(data, _additionalEntropy, DataProtectionScope.CurrentUser);
    }

    public byte[] Unprotect(byte[] data)
    {
        // Decrypt the data using DataProtectionScope.CurrentUser.
        return ProtectedData.Unprotect(data, _additionalEntropy, DataProtectionScope.CurrentUser);
    }
}