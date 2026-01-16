namespace Identity.Application.DTOs;

public class RegisterUserRequest
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string UserRole { get; set; }
}