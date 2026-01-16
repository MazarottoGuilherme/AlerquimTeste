namespace Identity.Application.DTOs;

public class LoginUserResponse
{
    public Guid UserId { get; set; }
    public string Nome { get; set; }
    public string Role { get; set; }
    public string Token { get; set; }

}