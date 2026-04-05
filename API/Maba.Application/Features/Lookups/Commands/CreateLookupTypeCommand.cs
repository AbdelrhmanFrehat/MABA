using MediatR;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Lookups.DTOs;
using Maba.Domain.Lookups;

namespace Maba.Application.Features.Lookups.Commands;

public class CreateLookupTypeCommand : IRequest<LookupTypeDto>
{
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateLookupTypeCommandHandler : IRequestHandler<CreateLookupTypeCommand, LookupTypeDto>
{
    private readonly IApplicationDbContext _context;

    public CreateLookupTypeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LookupTypeDto> Handle(CreateLookupTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = new LookupType
        {
            Id = Guid.NewGuid(),
            Key = request.Key,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Description = request.Description,
            IsSystem = false,
            IsActive = true
        };

        _context.Set<LookupType>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new LookupTypeDto
        {
            Id = entity.Id,
            Key = entity.Key,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
            Description = entity.Description,
            IsSystem = entity.IsSystem,
            IsActive = entity.IsActive
        };
    }
}
