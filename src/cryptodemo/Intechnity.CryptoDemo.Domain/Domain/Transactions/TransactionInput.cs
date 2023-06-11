using Intechnity.CryptoDemo.Core.Cryptography;

namespace Intechnity.CryptoDemo.Domain.Domain.Transactions;

public record TransactionInput
{
    public string? FromAddress { get; init; }

    public string? PreviousTransactionId { get; init; }

    public long PreviousBlockIndex { get; init; }

    public string Signature { get; private set; }

    public TransactionInput()
    {
    }

    public TransactionInput(string? fromAddress, string? previousTransactionId, long previousBlockIndex)
    {
        FromAddress = fromAddress;
        PreviousTransactionId = previousTransactionId;
        PreviousBlockIndex = previousBlockIndex;
    }

    public void Sign(byte[] protectedPrivateKey, IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        Signature = publicPrivateKeyGenerator.SignWithPrivateKey(PreviousTransactionId!, protectedPrivateKey);
    }

    public bool Validate(IPublicPrivateKeyGenerator publicPrivateKeyGenerator)
    {
        if (string.IsNullOrEmpty(PreviousTransactionId))
        {
            // transaction input from genesis block
            if (!string.IsNullOrWhiteSpace(FromAddress))
                return false;

            if (PreviousBlockIndex != -1)
                return false;

            if (!string.IsNullOrWhiteSpace(Signature))
                return false;

            return true;
        }

        if (string.IsNullOrWhiteSpace(FromAddress))
            return false;

        if (string.IsNullOrWhiteSpace(Signature))
            return false;

        var isSignatureValid = publicPrivateKeyGenerator.VerifySignature(PreviousTransactionId, Signature, FromAddress);

        return isSignatureValid;
    }
}