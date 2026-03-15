using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Auth.Commands;
using Maba.Domain.Users;
using System.Security.Cryptography;

namespace Maba.Application.Features.Auth.Handlers;

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, ResendVerificationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ResendVerificationCommandHandler(IApplicationDbContext context, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<ResendVerificationResult> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new ResendVerificationResult { Success = true, Message = "If the email exists, a new verification link has been sent." };
        }

        var emailLower = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, cancellationToken);

        if (user == null)
        {
            return new ResendVerificationResult { Success = true, Message = "If the email exists, a new verification link has been sent." };
        }

        if (user.EmailConfirmed)
        {
            return new ResendVerificationResult { Success = true, Message = "Email is already verified. You can sign in." };
        }

        var verificationToken = GenerateSecureToken();
        var apiBaseUrl = _configuration["App:ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
        var verificationLink = $"{apiBaseUrl}/api/v1/auth/verify-email?token={Uri.EscapeDataString(verificationToken)}";

        user.EmailVerificationToken = verificationToken;
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _emailService.SendEmailVerificationAsync(user.Email, verificationLink, cancellationToken);

        return new ResendVerificationResult { Success = true, Message = "A new verification link has been sent to your email." };
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
