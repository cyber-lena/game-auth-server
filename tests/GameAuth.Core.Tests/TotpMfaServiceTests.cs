using GameAuth.Core.Security;

namespace GameAuth.Core.Tests;

public class TotpMfaServiceTests
{
    private readonly TotpMfaService _mfa = new();

    [Fact]
    public void GenerateSecret_ProducesNonEmptyBase32Secret()
    {
        var secret = _mfa.GenerateSecret();

        Assert.False(string.IsNullOrWhiteSpace(secret));
    }

    [Fact]
    public void ValidateCode_ReturnsFalse_ForNullOrEmptyInputs()
    {
        var secret = _mfa.GenerateSecret();

        Assert.False(_mfa.ValidateCode(secret, null));
        Assert.False(_mfa.ValidateCode(secret, ""));
        Assert.False(_mfa.ValidateCode(null, "123456"));
    }

    [Fact]
    public void ValidateCode_ReturnsTrue_ForCurrentTotpCode()
    {
        var secret = _mfa.GenerateSecret();
        var key = OtpNet.Base32Encoding.ToBytes(secret);
        var code = new OtpNet.Totp(key).ComputeTotp();

        Assert.True(_mfa.ValidateCode(secret, code));
    }
}
