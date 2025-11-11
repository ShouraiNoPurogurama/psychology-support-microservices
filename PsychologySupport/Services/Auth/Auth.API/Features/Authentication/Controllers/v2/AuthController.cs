using Auth.API.Features.Authentication.Dtos.Requests;
using Auth.API.Features.Authentication.Dtos.Responses;
using Auth.API.Features.Authentication.ServiceContracts;
using Auth.API.Features.Authentication.ServiceContracts.Features;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.API.Features.Authentication.Controllers.v2;

[ApiController]
[Route("[controller]")]
public class AuthController(
    // IAuthService authService,
    IAuthFacade authService,
    IUserOnboardingService userOnboardingService,
    IUserSubscriptionService subscriptionService,
    IFirebaseAuthService firebaseAuthService) : ControllerBase
{
    [HttpPost("v2/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (!ModelState.IsValid) return BadRequest();
        var result = await authService.LoginAsync(loginRequest);
        return Ok(result);
    }

    [HttpPost("v2/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var result = await authService.RegisterAsync(registerRequest);
        return Ok(result);
    }

    // [Authorize(AuthenticationSchemes = "FirebaseAuth")]
    [HttpPost("v2/firebase/login")]
    public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
    {
        var response = await firebaseAuthService.FirebaseLoginAsync(request);
        return Ok(response);
    }

    [HttpPost("v2/google/login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await authService.GoogleLoginAsync(request);
        return Ok(result);
    }

    [HttpPost("v2/token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenApiRequest refreshTokenRequest)
    {
        var result = await authService.RefreshAsync(refreshTokenRequest);
        return Ok(result);
    }

    [HttpGet("v2/email/confirm")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailRequest confirmEmailRequest)
    {
        var result = await authService.ConfirmEmailAsync(confirmEmailRequest);
        return Redirect(result);
    }

    [HttpPost("v2/password/forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await authService.ForgotPasswordAsync(request.Email);
        return Ok(new { success = result, message = "Vui lòng kiểm tra email để đặt lại mật khẩu." });
    }

    [HttpPost("v2/password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await authService.ResetPasswordAsync(request);
        return Ok(new { success = result, message = "Đặt lại mật khẩu thành công." });
    }

    [HttpPost("v2/token/revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] TokenApiRequest request)
    {
        if (!ModelState.IsValid) return BadRequest();

        var result = await authService.RevokeAsync(request);
        return result ? Ok(new { message = "Token đã được thu hồi" }) : BadRequest("Thu hồi token thất bại");
    }

    [HttpPost("v2/password/change")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await authService.ChangePasswordAsync(request);
        return Ok(new { success = result, message = "Đổi mật khẩu thành công." });
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("v2/me/status")]
    public async Task<IActionResult> GetOnboardingStatus()
    {
        var onboardingStatus = await userOnboardingService.GetOnboardingStatusAsync();

        var aliasIssueStatus = await userOnboardingService.GetAliasIssueStatusAsync();

        var response = new OnboardingStatusDto(
            PiiCompleted: onboardingStatus.PiiCompleted,
            PatientProfileCompleted: onboardingStatus.PatientProfileCompleted,
            AliasIssued: aliasIssueStatus.AliasIssued
        );

        return Ok(response);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("v2/me/activate-free-trial")]
    public async Task<IActionResult> ActivateFreeTrial()
    {
        var subjectRefClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(subjectRefClaim) || !Guid.TryParse(subjectRefClaim, out var subjectRef))
        {
            return BadRequest(new { success = false, message = "JWT không chứa subjectRef hợp lệ." });
        }

        var result = await subscriptionService.ActivateFreeTrialAsync(subjectRef);

        if (!result)
            return BadRequest(new { success = false, message = "User đã sử dụng Free Trial trước đó hoặc không tồn tại." });

        return Ok(new { success = true, message = "Kích hoạt Free Trial thành công trong 3 ngày." });
    }
}