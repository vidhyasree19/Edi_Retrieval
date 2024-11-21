using EdiRetrieval.Models;
using EdiRetrieval.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EdiRetrieval.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IRegistrationService _registrationService;

    public AuthController(IAuthService authService, IRegistrationService registrationService)
    {
        _authService = authService;
        _registrationService = registrationService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Register register)
    {
        if (string.IsNullOrEmpty(register.Email) || string.IsNullOrEmpty(register.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var resultMessage = await _registrationService.RegisterUserAsync(register);

        if (resultMessage == "User registered successfully!")
        {
            return Ok(new { message = resultMessage });
        }
        else
        {
            return BadRequest(new { message = resultMessage });
        }
    }

    // Ensure this is marked as HttpPost
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login loginModel)
    {
        if (loginModel == null || string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        var token = await _authService.AuthenticateAsync(loginModel.Email, loginModel.Password);
        if (token == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        return Ok(new { token });
    }
}

}
