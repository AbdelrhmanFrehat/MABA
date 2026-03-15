using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Maba.Application.Features.Auth.Commands;
using Maba.Application.Features.Auth.DTOs;
using Maba.Application.Features.Auth.Queries;
using System.Security.Claims;
using System.Net;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpGet("verify-email")]
    [Produces("text/html")]
    public async Task<IActionResult> VerifyEmailGet([FromQuery] string? token)
    {
        var result = await _mediator.Send(new VerifyEmailCommand { Token = token });
        var frontendBase = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        var loginUrl = $"{frontendBase}/auth/login";
        if (result.Success)
        {
            var html = $@"<!DOCTYPE html><html><head><meta charset=""UTF-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/><title>Email verified - MABA</title><style>body{{font-family:system-ui,sans-serif;margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;background:linear-gradient(135deg,#0c1445 0%,#1a1a2e 50%,#16213e 100%);color:#fff;padding:1rem;}}.card{{background:rgba(255,255,255,.95);color:#1a1a2e;border-radius:24px;padding:2.5rem;max-width:420px;width:100%;text-align:center;box-shadow:0 25px 50px rgba(0,0,0,.3);}}.icon{{width:72px;height:72px;margin:0 auto 1.5rem;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:2rem;background:rgba(34,197,94,.2);color:#16a34a;}}.card h1{{font-size:1.5rem;margin:0 0 .5rem;}}.card p{{color:#6c757d;margin:0 0 1.5rem;}}.card a{{display:inline-flex;align-items:center;gap:.5rem;padding:.75rem 1.5rem;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);color:#fff;text-decoration:none;border-radius:12px;font-weight:600;}}</style></head><body><div class=""card""><div class=""icon"">✓</div><h1>Email verified</h1><p>Your email has been verified. You can now sign in.</p><a href=""{loginUrl}"">Go to Sign in</a></div></body></html>";
            return Content(html, "text/html");
        }
        var errMsg = WebUtility.HtmlEncode(result.Message);
        var errorHtml = $@"<!DOCTYPE html><html><head><meta charset=""UTF-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/><title>Verification failed - MABA</title><style>body{{font-family:system-ui,sans-serif;margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;background:linear-gradient(135deg,#0c1445 0%,#1a1a2e 50%,#16213e 100%);color:#fff;padding:1rem;}}.card{{background:rgba(255,255,255,.95);color:#1a1a2e;border-radius:24px;padding:2.5rem;max-width:420px;width:100%;text-align:center;box-shadow:0 25px 50px rgba(0,0,0,.3);}}.icon{{width:72px;height:72px;margin:0 auto 1.5rem;border-radius:50%;display:flex;align-items:center;justify-content:center;font-size:2rem;background:rgba(239,68,68,.15);color:#dc2626;}}.card h1{{font-size:1.5rem;margin:0 0 .5rem;}}.card p{{color:#6c757d;margin:0 0 1.5rem;}}.card a{{display:inline-flex;align-items:center;gap:.5rem;padding:.75rem 1.5rem;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);color:#fff;text-decoration:none;border-radius:12px;font-weight:600;}}</style></head><body><div class=""card""><div class=""icon"">✕</div><h1>Verification failed</h1><p>{errMsg}</p><a href=""{loginUrl}"">Go to Sign in</a></div></body></html>";
        return Content(errorHtml, "text/html");
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return StatusCode(201, result);
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<VerifyEmailResult>> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(new { error = result.Message });
        return Ok(result);
    }

    [HttpPost("resend-verification")]
    public async Task<ActionResult<ResendVerificationResult>> ResendVerification([FromBody] ResendVerificationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "If the email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Password has been reset successfully." });
    }

    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        command.UserId = userId;
        await _mediator.Send(command);
        return Ok(new { message = "Password has been changed successfully." });
    }

    [HttpPut("email")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> UpdateEmail([FromBody] UpdateEmailCommand command)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        command.UserId = userId;
        await _mediator.Send(command);
        return Ok(new { message = "Email has been updated. Please verify your new email." });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());
        var query = new Maba.Application.Features.Auth.Queries.GetCurrentUserQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

