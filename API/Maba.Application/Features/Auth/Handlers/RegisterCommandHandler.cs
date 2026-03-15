using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Auth.Commands;
using Maba.Application.Features.Auth.DTOs;
using Maba.Domain.Users;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Maba.Application.Features.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(IApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var buyerRole = await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name == "Buyer", cancellationToken);

        if (buyerRole == null)
        {
            throw new InvalidOperationException("Buyer role not found");
        }

        var verificationToken = GenerateSecureToken();
        var apiBaseUrl = _configuration["App:ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
        var verificationLink = $"{apiBaseUrl}/api/v1/auth/verify-email?token={Uri.EscapeDataString(verificationToken)}";

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            EmailConfirmed = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _context.Set<User>().Add(user);

        _context.Set<UserRole>().Add(new UserRole
        {
            UserId = user.Id,
            RoleId = buyerRole.Id
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailVerificationAsync(user.Email, verificationLink, cancellationToken);

        return new RegisterResponseDto
        {
            Message = "Registration successful. Please check your email to verify your account.",
            CheckEmailUrl = $"{apiBaseUrl}/check-email"
        };
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
