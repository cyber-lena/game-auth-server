using GameAuth.Core.Security;

namespace GameAuth.Core.Tests;

public class Argon2PasswordHasherTests
{
    private readonly Argon2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesVerifiableHash()
    {
        var hash = _hasher.Hash("S3cur3P@ss");

        Assert.NotNull(hash);
        Assert.True(_hasher.Verify("S3cur3P@ss", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hash = _hasher.Hash("correct-horse");

        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_ProducesDifferentHashes_ForSamePassword()
    {
        var first = _hasher.Hash("same-password");
        var second = _hasher.Hash("same-password");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_ReturnsFalse_ForMalformedHash()
    {
        Assert.False(_hasher.Verify("anything", "not-a-valid-hash"));
    }
}
