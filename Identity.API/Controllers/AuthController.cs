using Identity.Application.DTOs;
using Identity.Application.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
    {
        try
        {
            var token =  _authService.Login(request);
            return Ok(new { Token = token });
        }
        catch(Exception ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }
}