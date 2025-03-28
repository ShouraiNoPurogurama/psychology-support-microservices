using Auth.API.Dtos.Requests;
using Auth.API.Models;
using Auth.API.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IConfiguration configuration,
    IAuthService authService,
    IFirebaseAuthService firebaseAuthService) : ControllerBase
{
    // [Authorize(AuthenticationSchemes = "LocalAuth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (!ModelState.IsValid) return BadRequest();
        var result = await authService.LoginAsync(loginRequest);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var result = await authService.RegisterAsync(registerRequest);
        return Ok(result);
    }

    // [Authorize(AuthenticationSchemes = "FirebaseAuth")]
    [HttpPost("firebase-login")]
    public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
    {
        var response = await firebaseAuthService.FirebaseLoginAsync(request);
        return Ok(response);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenApiRequest refreshTokenRequest)
    {
        var result = await authService.RefreshAsync(refreshTokenRequest);
        return Ok(result);
    }
}