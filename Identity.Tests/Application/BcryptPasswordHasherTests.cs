using Identity.Application.Services;

namespace Identity.Tests.Application;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher;

    public BcryptPasswordHasherTests()
    {
        _hasher = new BcryptPasswordHasher();
    }

    [Fact]
    public void Hash_ValidPassword_ShouldReturnHash()
    {
        var password = "password123";

        var hash = _hasher.Hash(password);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash);
        Assert.True(hash.StartsWith("$2"));
    }

    [Fact]
    public void Hash_SamePassword_ShouldReturnDifferentHashes()
    {
        var password = "password123";

        var hash1 = _hasher.Hash(password);
        var hash2 = _hasher.Hash(password);

        Assert.NotEqual(hash1, hash2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Hash_InvalidPassword_ShouldThrowArgumentException(string invalidPassword)
    {
        var ex = Assert.Throws<ArgumentException>(() => _hasher.Hash(invalidPassword));
        Assert.Equal("Password cannot be empty", ex.Message);
    }

    [Fact]
    public void Verify_CorrectPassword_ShouldReturnTrue()
    {
        var password = "password123";
        var hash = _hasher.Hash(password);

        var result = _hasher.Verify(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_WrongPassword_ShouldReturnFalse()
    {
        var password = "password123";
        var wrongPassword = "wrongpassword";
        var hash = _hasher.Hash(password);

        var result = _hasher.Verify(wrongPassword, hash);

        Assert.False(result);
    }

    [Fact]
    public void Verify_EmptyPassword_ShouldReturnFalse()
    {
        var password = "password123";
        var hash = _hasher.Hash(password);

        var result = _hasher.Verify("", hash);

        Assert.False(result);
    }

    [Fact]
    public void Verify_EmptyHash_ShouldReturnFalse()
    {
        var result = _hasher.Verify("password123", "");

        Assert.False(result);
    }

    [Fact]
    public void Verify_NullPassword_ShouldReturnFalse()
    {
        var password = "password123";
        var hash = _hasher.Hash(password);

        var result = _hasher.Verify(null, hash);

        Assert.False(result);
    }

    [Fact]
    public void Verify_NullHash_ShouldReturnFalse()
    {
        var result = _hasher.Verify("password123", null);

        Assert.False(result);
    }
}

