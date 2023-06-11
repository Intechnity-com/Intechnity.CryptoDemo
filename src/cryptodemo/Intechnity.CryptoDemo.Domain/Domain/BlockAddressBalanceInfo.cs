namespace Intechnity.CryptoDemo.Domain.Domain;

public class BlockAddressBalanceInfo
{
    public Dictionary<string, decimal> AddressBalance { get; } = new Dictionary<string, decimal>();

    public bool HasBalanceForAddress(string address)
    {
        return AddressBalance.ContainsKey(address);
    }

    public void SetAddressBalance(string address, decimal balance)
    {
        AddressBalance[address] = balance;
    }

    public decimal GetAddressBalance(string address)
    {
        return AddressBalance[address];
    }
}