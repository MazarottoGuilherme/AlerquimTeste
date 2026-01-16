using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain;

namespace Identity.Application.Services;

public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _hasher;
    
    public UserService(IUserRepository repository, IPasswordHasher hasher)
    {
        _repository = repository;
        _hasher = hasher;
    }

    public UserDTO RegisterUser(RegisterUserRequest request)
    {
        if (_repository.GetByEmail(request.Email) != null)
            throw new DomainException("Email já registrado");

        if (!Enum.TryParse<UserRole>(request.UserRole, true, out var role))
            throw new DomainException("Role invalida");
        
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new DomainException("Senha é obrigatoria");
        
        if (request.Password.Length < 6)
            throw new DomainException("Senha deve ter no mínimo 6 caracteres");
        
        var email = Email.Create(request.Email);
        var passwordHash = _hasher.Hash(request.Password);

        var user = UserDomain.Register(request.Nome, email, passwordHash, role);

        _repository.Add(user);

        return new UserDTO
        {
            Id = user.Id.Value,
            Nome = user.Nome,
            Email = user.Email.Value,
            Role = user.Role.ToString()
        };
    }
    
    public LoginUserResponse Login(LoginUserRequest request)
    {
        var user = _repository.GetByEmail(request.Email);
        if (user == null)
            throw new DomainException("Email ou senha invalidos");

        if (!_hasher.Verify(request.Password, user.Password))
            throw new DomainException("Email ou senha invalidos");

        return new LoginUserResponse
        {
            UserId = user.Id.Value,
            Nome = user.Nome,
            Role = user.Role.ToString()
        };
    }


}