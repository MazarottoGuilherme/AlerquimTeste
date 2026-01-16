using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _hasher;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepository, IPasswordHasher hasher, IConfiguration config)
    {
        _userRepository = userRepository;
        _hasher = hasher;
        _config = config;
    }

    public LoginUserResponse Login(LoginUserRequest request)
    {
        var user =  _userRepository.GetByEmail(request.Email);
        if (user == null || !_hasher.Verify(request.Password, user.Password))
            throw new DomainException("Email ou senha invalidos");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new Claim(ClaimTypes.Name, user.Nome),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new LoginUserResponse
        {
            UserId = user.Id.Value,
            Nome = user.Nome,
            Role = user.Role.ToString(),
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };
    }
}
