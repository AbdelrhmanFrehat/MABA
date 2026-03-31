using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Auth.Commands;
using Maba.Domain.Users;

namespace Maba.Application.Features.Auth.Handlers;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public ResetPasswordCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var query = _context.Set<User>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLowerInvariant();
            query = query.Where(u => u.Email.ToLower() == email);
        }
        else
        {
            query = query.Where(u => u.PasswordResetToken == request.Token);
        }

        var user = await query.FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("Invalid reset token.");
        }

        // Validate token
        if (user.PasswordResetToken != request.Token ||
            user.PasswordResetExpiresAt == null ||
            user.PasswordResetExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired reset token.");
        }

        // Validate password strength
        if (request.NewPassword.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

