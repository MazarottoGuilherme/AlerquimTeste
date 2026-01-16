using System.IdentityModel.Tokens.Jwt;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Domain;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Identity.Tests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _configMock = new Mock<IConfiguration>();
        
        _configMock.Setup(c => c["Jwt:Key"]).Returns("7xCtKCvsGEqyFaZwkuCZBIwQGbpb9pIXflTX14Zc7Ua");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("MyApp");
        
        _service = new AuthService(_userRepoMock.Object, _hasherMock.Object, _configMock.Object);
    }

    [Fact]
    public void Login_ValidCredentials_ShouldReturnTokenAndUserData()
    {
        var request = new LoginUserRequest
        {
            Email = "exemplodeteste@email.com",
            Password = "password123"
        };

        var user = UserDomain.Register(
            "Exemplo de teste",
            Email.Create("exemplodeteste@email.com"),
            "hashedPassword",
            UserRole.Seller
        );

        _userRepoMock.Setup(r => r.GetByEmail(request.Email)).Returns(user);
        _hasherMock.Setup(h => h.Verify(request.Password, user.Password)).Returns(true);

        var result = _service.Login(request);

        Assert.NotNull(result);
        Assert.Equal(user.Id.Value, result.UserId);
        Assert.Equal(user.Nome, result.Nome);
        Assert.Equal(user.Role.ToString(), result.Role);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
        
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(result.Token));
    }

    [Fact]
    public void Login_InvalidEmail_ShouldThrowDomainException()
    {
        var request = new LoginUserRequest
        {
            Email = "inexistente@email.com",
            Password = "password123"
        };

        _userRepoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);

        var ex = Assert.Throws<DomainException>(() => _service.Login(request));
        Assert.Equal("Email ou senha invalidos", ex.Message);
    }

    [Fact]
    public void Login_InvalidPassword_ShouldThrowDomainException()
    {
        var request = new LoginUserRequest
        {
            Email = "exemplodeteste@email.com",
            Password = "wrongpassword"
        };

        var user = UserDomain.Register(
            "Exemplo de teste",
            Email.Create("exemplodeteste@email.com"),
            "hashedPassword",
            UserRole.Seller
        );

        _userRepoMock.Setup(r => r.GetByEmail(request.Email)).Returns(user);
        _hasherMock.Setup(h => h.Verify(request.Password, user.Password)).Returns(false);

        var ex = Assert.Throws<DomainException>(() => _service.Login(request));
        Assert.Equal("Email ou senha invalidos", ex.Message);
    }

    [Fact]
    public void Login_TokenShouldContainCorrectClaims()
    {
        var request = new LoginUserRequest
        {
            Email = "exemplodeteste@email.com",
            Password = "password123"
        };

        var user = UserDomain.Register(
            "Exemplo de teste",
            Email.Create("exemplodeteste@email.com"),
            "hashedPassword",
            UserRole.Admin
        );

        _userRepoMock.Setup(r => r.GetByEmail(request.Email)).Returns(user);
        _hasherMock.Setup(h => h.Verify(request.Password, user.Password)).Returns(true);

        var result = _service.Login(request);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);
        
        Assert.Equal(user.Id.Value.ToString(), token.Subject);
        Assert.Equal(user.Nome, token.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Name).Value);
        Assert.Equal(user.Role.ToString(), token.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value);
        Assert.Equal("MyApp", token.Issuer);
    }
}

