using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Auth.Commands;
using Maba.Domain.Users;

namespace Maba.Application.Features.Auth.Handlers;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(IApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            // Don't reveal if user exists - security best practice
            return Unit.Value;
        }

        // Generate reset token (simplified - in production use cryptographically secure token)
        var token = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = token;
        user.PasswordResetExpiresAt = DateTime.UtcNow.AddHours(24); // Token valid for 24 hours
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var frontendBaseUrl = _configuration["App:FrontendBaseUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
        var resetLink = $"{frontendBaseUrl}/auth/reset-password?token={Uri.EscapeDataString(token)}";
        await _emailService.SendPasswordResetAsync(user.Email, resetLink, cancellationToken);

        return Unit.Value;
    }
}

