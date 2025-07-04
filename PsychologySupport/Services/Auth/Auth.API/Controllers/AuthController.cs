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
    
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await authService.GoogleLoginAsync(request);
        return Ok(result);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenApiRequest refreshTokenRequest)
    {
        var result = await authService.RefreshAsync(refreshTokenRequest);
        return Ok(result);
    }
    
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequest confirmEmailRequest)
    {
        var result = await authService.ConfirmEmailAsync(confirmEmailRequest);
        return Redirect(result);
    }
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await authService.ForgotPasswordAsync(request.Email);
        return Ok(new { success = result, message = "Vui lòng kiểm tra email để đặt lại mật khẩu." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await authService.ResetPasswordAsync(request);
        return Ok(new { success = result, message = "Đặt lại mật khẩu thành công." });
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] TokenApiRequest request)
    {
        if (!ModelState.IsValid) return BadRequest();

        var result = await authService.RevokeAsync(request);
        return result ? Ok(new { message = "Token đã được thu hồi" }) : BadRequest("Thu hồi token thất bại");
    }

}