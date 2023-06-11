namespace Intechnity.CryptoDemo.Core.Providers;

public class DummyTranslationProvider : ITranslationProvider
{
    public string Translate(string input, params object?[] args) => string.Format(input, args);
}