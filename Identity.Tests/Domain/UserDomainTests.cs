using Identity.Domain;

namespace Identity.Tests.Domain;

public class UserDomainTests
{
    [Fact]
    public void Register_ValidData_ShouldCreateUser()
    {
        var name = "Exemplo de teste";
        var email = Email.Create("exemplodeteste@email.com");
        var passwordHash = "hashedPassword";
        var role = UserRole.Seller;

        var user = UserDomain.Register(name, email, passwordHash, role);

        Assert.NotNull(user);
        Assert.Equal(name, user.Nome);
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.Password);
        Assert.Equal(role, user.Role);
        Assert.NotEqual(Guid.Empty, user.Id.Value);
    }

    [Theory]
    [InlineData("", "exemplodeteste@email.com", "hashed", UserRole.Seller)]
    [InlineData("Exemplo de teste", null, "hashed", UserRole.Seller)]
    [InlineData("Exemplo de teste", "exemplodeteste@email.com", "", UserRole.Seller)]
    public void Register_InvalidData_ShouldThrowDomainException(string name, string emailStr, string password,
        UserRole role)
    {
        Email email = emailStr != null ? Email.Create(emailStr) : null;

        Assert.Throws<DomainException>(() => UserDomain.Register(name, email, password, role));
    }

    [Fact]
    public void ChangeRole_AdminToSeller_ShouldThrowDomainException()
    {
        var user = UserDomain.Register(
            "Exemplo de teste",
            Email.Create("exemplodeteste@email.com"),
            "hashedPassword",
            UserRole.Admin
        );

        var ex = Assert.Throws<DomainException>(() => user.ChangeRole(UserRole.Seller));
        Assert.Equal("Admin não pode mudar de cargo", ex.Message);
    }

    [Fact]
    public void ChangeRole_ValidChange_ShouldUpdateRole()
    {
        var user = UserDomain.Register(
            "Exemplo de teste",
            Email.Create("exemplodeteste@email.com"),
            "hashedPassword",
            UserRole.Seller
        );

        user.ChangeRole(UserRole.Admin);

        Assert.Equal(UserRole.Admin, user.Role);
    }
}