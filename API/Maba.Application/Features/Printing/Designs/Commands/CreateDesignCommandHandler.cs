using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Printing.Designs.Commands;
using Maba.Application.Features.Printing.DTOs;
using Maba.Domain.Printing;

namespace Maba.Application.Features.Printing.Designs.Handlers;

public class CreateDesignCommandHandler : IRequestHandler<CreateDesignCommand, DesignDto>
{
    private readonly IApplicationDbContext _context;

    public CreateDesignCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DesignDto> Handle(CreateDesignCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Set<Domain.Users.User>()
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            throw new KeyNotFoundException("User not found");
        }

        var design = new Design
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Notes = request.Notes
        };

        _context.Set<Design>().Add(design);
        await _context.SaveChangesAsync(cancellationToken);

        return new DesignDto
        {
            Id = design.Id,
            UserId = design.UserId,
            Title = design.Title,
            Notes = design.Notes,
            Files = new List<DesignFileDto>(),
            CreatedAt = design.CreatedAt,
            UpdatedAt = design.UpdatedAt
        };
    }
}

