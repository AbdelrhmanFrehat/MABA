namespace Maba.Application.Features.Auth.DTOs;

/// <summary>
/// Response for registration when email verification is required. No token is issued until email is verified.
/// </summary>
public class RegisterResponseDto
{
    public string Message { get; set; } = "Registration successful. Please check your email to verify your account.";
    /// <summary>URL to redirect the user to (check-email page on the API).</summary>
    public string? CheckEmailUrl { get; set; }
}
