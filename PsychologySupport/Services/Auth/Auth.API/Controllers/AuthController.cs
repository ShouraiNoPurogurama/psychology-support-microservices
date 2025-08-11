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

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await authService.ChangePasswordAsync(request);
        return Ok(new { success = result, message = "Đổi mật khẩu thành công." });
    }

    // Refactor  API

    [HttpPost("v2/login")]
    public async Task<IActionResult> Login1([FromBody] LoginRequest loginRequest)
    {
        if (!ModelState.IsValid) return BadRequest();
        var result = await authService.LoginAsync(loginRequest);
        return Ok(result);
    }

    [HttpPost("v2/register")]
    public async Task<IActionResult> Register1([FromBody] RegisterRequest registerRequest)
    {
        var result = await authService.RegisterAsync(registerRequest);
        return Ok(result);
    }

    // [Authorize(AuthenticationSchemes = "FirebaseAuth")]
    [HttpPost("v2/firebase/login")]
    public async Task<IActionResult> FirebaseLogin1([FromBody] FirebaseLoginRequest request)
    {
        var response = await firebaseAuthService.FirebaseLoginAsync(request);
        return Ok(response);
    }

    [HttpPost("v2/google/login")]
    public async Task<IActionResult> GoogleLogin1([FromBody] GoogleLoginRequest request)
    {
        var result = await authService.GoogleLoginAsync(request);
        return Ok(result);
    }

    [HttpPost("v2/token/refresh")]
    public async Task<IActionResult> RefreshToken1([FromBody] TokenApiRequest refreshTokenRequest)
    {
        var result = await authService.RefreshAsync(refreshTokenRequest);
        return Ok(result);
    }

    [HttpGet("v2/email/confirm")]
    public async Task<IActionResult> ConfirmEmail1([FromQuery] ConfirmEmailRequest confirmEmailRequest)
    {
        var result = await authService.ConfirmEmailAsync(confirmEmailRequest);
        return Redirect(result);
    }

    [HttpPost("v2/password/forgot")]
    public async Task<IActionResult> ForgotPassword1([FromBody] ForgotPasswordRequest request)
    {
        var result = await authService.ForgotPasswordAsync(request.Email);
        return Ok(new { success = result, message = "Vui lòng kiểm tra email để đặt lại mật khẩu." });
    }

    [HttpPost("v2/password/reset")]
    public async Task<IActionResult> ResetPassword1([FromBody] ResetPasswordRequest request)
    {
        var result = await authService.ResetPasswordAsync(request);
        return Ok(new { success = result, message = "Đặt lại mật khẩu thành công." });
    }

    [HttpPost("v2/token/revoke")]
    public async Task<IActionResult> RevokeToken1([FromBody] TokenApiRequest request)
    {
        if (!ModelState.IsValid) return BadRequest();

        var result = await authService.RevokeAsync(request);
        return result ? Ok(new { message = "Token đã được thu hồi" }) : BadRequest("Thu hồi token thất bại");
    }

    [HttpPost("v2/password/change")]
    public async Task<IActionResult> ChangePassword1([FromBody] ChangePasswordRequest request)
    {
        var result = await authService.ChangePasswordAsync(request);
        return Ok(new { success = result, message = "Đổi mật khẩu thành công." });
    }

}