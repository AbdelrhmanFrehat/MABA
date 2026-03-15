using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Auth.Commands;
using Maba.Domain.Users;
using System.Security.Cryptography;
using System.Text;

namespace Maba.Application.Features.Auth.Handlers;

public class UpdateEmailCommandHandler : IRequestHandler<UpdateEmailCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        // Verify password
        var passwordHash = HashPassword(request.Password);
        if (user.PasswordHash != passwordHash)
        {
            throw new UnauthorizedAccessException("Password is incorrect.");
        }

        // Check if new email already exists
        var existingUser = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.NewEmail && u.Id != request.UserId, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already in use.");
        }

        // Update email
        user.Email = request.NewEmail;
        user.EmailConfirmed = false; // Require re-verification
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Send verification email

        return Unit.Value;
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}

