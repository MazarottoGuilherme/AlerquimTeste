using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Domain;
using Moq;

namespace Identity.Tests.Application;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _service = new UserService(_repoMock.Object, _hasherMock.Object);
    }

    [Fact]
    public void RegisterUser_ValidRequest_ShouldReturnUserDTO()
    {
        var request = new RegisterUserRequest
        {
            Nome = "Exemplo de teste",
            Email = "exemplodeteste@email.com",
            Password = "password123",
            UserRole = "Seller"
        };

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);
        _hasherMock.Setup(h => h.Hash(request.Password)).Returns("hashedPassword");

        var result = _service.RegisterUser(request);

        Assert.NotNull(result);
        Assert.Equal(request.Nome, result.Nome);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.UserRole, result.Role);

        _repoMock.Verify(r => r.Add(It.IsAny<UserDomain>()), Times.Once);
        _hasherMock.Verify(h => h.Hash(request.Password), Times.Once);
    }

    [Fact]
    public void RegisterUser_WithAdminRole_ShouldReturnUserDTOWithAdminRole()
    {
        var request = new RegisterUserRequest
        {
            Nome = "Exemplo de teste",
            Email = "exemplodeteste@email.com",
            Password = "password123",
            UserRole = "Admin"
        };

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);
        _hasherMock.Setup(h => h.Hash(request.Password)).Returns("hashedPassword");

        var result = _service.RegisterUser(request);

        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public void RegisterUser_EmailAlreadyExists_ShouldThrowDomainException()
    {
        var request = new RegisterUserRequest
        {
            Nome = "Exemplo de teste",
            Email = "exemplodeteste@email.com",
            Password = "password123",
            UserRole = "Seller"
        };

        var existingUser = UserDomain.Register(
            "Exemplo de teste",
            Email.Create("exemplodeteste@email.com"),
            "hashedPassword",
            UserRole.Seller
        );

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns(existingUser);

        var ex = Assert.Throws<DomainException>(() => _service.RegisterUser(request));
        Assert.Equal("Email já registrado", ex.Message);

        _repoMock.Verify(r => r.Add(It.IsAny<UserDomain>()), Times.Never);
    }

    [Fact]
    public void RegisterUser_InvalidRole_ShouldThrowDomainException()
    {
        var request = new RegisterUserRequest
        {
            Nome = "Exemplo de teste",
            Email = "exemplodeteste@email.com",
            Password = "password123",
            UserRole = "InvalidRole"
        };

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);

        var ex = Assert.Throws<DomainException>(() => _service.RegisterUser(request));
        Assert.Equal("Role invalida", ex.Message);
    }

    [Fact]
    public void RegisterUser_PasswordEmpty_ShouldThrowDomainException()
    {
        var request = new RegisterUserRequest
        {
            Nome = "Exemplo de teste",
            Email = "exemplodeteste@email.com",
            Password = "",
            UserRole = "Seller"
        };

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);

        var ex = Assert.Throws<DomainException>(() => _service.RegisterUser(request));
        Assert.Equal("Senha é obrigatoria", ex.Message);

        _repoMock.Verify(r => r.Add(It.IsAny<UserDomain>()), Times.Never);
        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void RegisterUser_PasswordNull_ShouldThrowDomainException()
    {
        var request = new RegisterUserRequest
        {
            Nome = "Exemplo de teste",
            Email = "exemplodeteste@email.com",
            Password = null,
            UserRole = "Seller"
        };

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);

        var ex = Assert.Throws<DomainException>(() => _service.RegisterUser(request));
        Assert.Equal("Senha é obrigatoria", ex.Message);

        _repoMock.Verify(r => r.Add(It.IsAny<UserDomain>()), Times.Never);
        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abc")]
    [InlineData("12")]
    [InlineData("1")]
    public void RegisterUser_PasswordLessThan6Characters_ShouldThrowDomainException(string shortPassword)
    {
        var request = new RegisterUserRequest
        {
            Nome = "Exemplo de teste",
            Email = "exemplodeteste@email.com",
            Password = shortPassword,
            UserRole = "Seller"
        };

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);

        var ex = Assert.Throws<DomainException>(() => _service.RegisterUser(request));
        Assert.Equal("Senha deve ter no mínimo 6 caracteres", ex.Message);

        _repoMock.Verify(r => r.Add(It.IsAny<UserDomain>()), Times.Never);
        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void RegisterUser_PasswordExactly6Characters_ShouldSucceed()
    {
        var request = new RegisterUserRequest
        {
            Nome = "Exemplo de teste",
            Email = "exemplodeteste@email.com",
            Password = "123456", 
            UserRole = "Seller"
        };

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);
        _hasherMock.Setup(h => h.Hash(request.Password)).Returns("hashedPassword");

        var result = _service.RegisterUser(request);

        Assert.NotNull(result);
        _repoMock.Verify(r => r.Add(It.IsAny<UserDomain>()), Times.Once);
        _hasherMock.Verify(h => h.Hash(request.Password), Times.Once);
    }

    [Fact]
    public void RegisterUser_PasswordMoreThan6Characters_ShouldSucceed()
    {
        var request = new RegisterUserRequest
        {
            Nome = "Exemplo de teste",
            Email = "exemplodeteste@email.com",
            Password = "password123",
            UserRole = "Seller"
        };

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);
        _hasherMock.Setup(h => h.Hash(request.Password)).Returns("hashedPassword");

        var result = _service.RegisterUser(request);

        Assert.NotNull(result);
        _repoMock.Verify(r => r.Add(It.IsAny<UserDomain>()), Times.Once);
        _hasherMock.Verify(h => h.Hash(request.Password), Times.Once);
    }

    [Fact]
    public void Login_ValidCredentials_ShouldReturnLoginResponse()
    {
        var request = new LoginUserRequest
        {
            Email = "john@example.com",
            Password = "password123"
        };

        var user = UserDomain.Register(
            "Exemplo de teste",
            Email.Create("exemplodeteste@email.com"),
            "hashedPassword",
            UserRole.Seller
        );

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns(user);
        _hasherMock.Setup(h => h.Verify(request.Password, user.Password)).Returns(true);

        var result = _service.Login(request);

        Assert.NotNull(result);
        Assert.Equal(user.Id.Value, result.UserId);
        Assert.Equal(user.Nome, result.Nome);
        Assert.Equal(user.Role.ToString(), result.Role);
    }

    [Fact]
    public void Login_InvalidEmail_ShouldThrowDomainException()
    {
        var request = new LoginUserRequest
        {
            Email = "inexistente@email.com",
            Password = "password123"
        };

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns((UserDomain)null);

        var ex = Assert.Throws<DomainException>(() => _service.Login(request));
        Assert.Equal("Email ou senha invalidos", ex.Message);
    }

    [Fact]
    public void Login_InvalidPassword_ShouldThrowDomainException()
    {
        var request = new LoginUserRequest
        {
            Email = "john@example.com",
            Password = "wrongpassword"
        };

        var user = UserDomain.Register(
            "Exemplo de teste",
            Email.Create("exemplodeteste@email.com"),
            "hashedPassword",
            UserRole.Seller
        );

        _repoMock.Setup(r => r.GetByEmail(request.Email)).Returns(user);
        _hasherMock.Setup(h => h.Verify(request.Password, user.Password)).Returns(false);

        var ex = Assert.Throws<DomainException>(() => _service.Login(request));
        Assert.Equal("Email ou senha invalidos", ex.Message);
    }
}

