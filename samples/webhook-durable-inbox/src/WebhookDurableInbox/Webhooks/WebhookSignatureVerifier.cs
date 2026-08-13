using System.Security.Cryptography;
using System.Text;

namespace WebhookDurableInbox.Webhooks;

internal sealed class WebhookSignatureVerifier(string secret)
{
    private readonly byte[] _key = Encoding.UTF8.GetBytes(secret);

    public bool IsValid(ReadOnlySpan<byte> payload, string signature)
    {
        const string prefix = "sha256=";
        if (!signature.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
        var suppliedHex = signature[prefix.Length..];
        if (suppliedHex.Length != 64) return false;

        byte[] supplied;
        try
        {
            supplied = Convert.FromHexString(suppliedHex);
        }
        catch (FormatException)
        {
            return false;
        }

        var expected = HMACSHA256.HashData(_key, payload);
        return CryptographicOperations.FixedTimeEquals(expected, supplied);
    }
}
