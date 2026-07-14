using OtpNet;

namespace GameAuth.Core.Security;

public interface IMfaService
{
    string GenerateSecret();
    bool ValidateCode(string? secret, string? code);
}

public class TotpMfaService : IMfaService
{
    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public bool ValidateCode(string? secret, string? code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
    }
}
