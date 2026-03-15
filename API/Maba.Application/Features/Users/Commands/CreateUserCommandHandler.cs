using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Users.Commands;
using Maba.Application.Features.Users.DTOs;
using Maba.Domain.Users;
using System.Security.Cryptography;
using System.Text;

namespace Maba.Application.Features.Users.Handlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;

    public CreateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        // Validate roles exist
        if (request.RoleIds.Any())
        {
            var roles = await _context.Set<Role>()
                .Where(r => request.RoleIds.Contains(r.Id))
                .ToListAsync(cancellationToken);

            if (roles.Count != request.RoleIds.Count)
            {
                throw new KeyNotFoundException("One or more roles not found.");
            }
        }

        // Hash password
        var passwordHash = HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = passwordHash,
            IsActive = request.IsActive,
            EmailConfirmed = false,
            PhoneConfirmed = false,
            LockoutEnabled = true,
            Street = request.Street,
            City = request.City,
            Country = request.Country,
            PostalCode = request.PostalCode
        };

        _context.Set<User>().Add(user);

        // Assign roles
        foreach (var roleId in request.RoleIds)
        {
            _context.Set<UserRole>().Add(new UserRole
            {
                UserId = user.Id,
                RoleId = roleId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Load user with roles
        var userWithRoles = await _context.Set<User>()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        return new UserDto
        {
            Id = userWithRoles!.Id,
            FullName = userWithRoles.FullName,
            Email = userWithRoles.Email,
            Phone = userWithRoles.Phone,
            IsActive = userWithRoles.IsActive,
            Roles = userWithRoles.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt = userWithRoles.CreatedAt,
            UpdatedAt = userWithRoles.UpdatedAt
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}

