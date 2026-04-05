using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Lookups.DTOs;
using Maba.Domain.Lookups;

namespace Maba.Application.Features.Lookups.Commands;

public class CreateLookupValueCommand : IRequest<LookupValueDto>
{
    public Guid LookupTypeId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateLookupValueCommandHandler : IRequestHandler<CreateLookupValueCommand, LookupValueDto>
{
    private readonly IApplicationDbContext _context;

    public CreateLookupValueCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LookupValueDto> Handle(CreateLookupValueCommand request, CancellationToken cancellationToken)
    {
        var lookupTypeExists = await _context.Set<LookupType>()
            .AnyAsync(x => x.Id == request.LookupTypeId, cancellationToken);

        if (!lookupTypeExists)
        {
            throw new KeyNotFoundException("Lookup type not found.");
        }

        var entity = new LookupValue
        {
            Id = Guid.NewGuid(),
            LookupTypeId = request.LookupTypeId,
            Key = request.Key,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsDefault = request.IsDefault,
            IsSystem = false,
            IsActive = true
        };

        _context.Set<LookupValue>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new LookupValueDto
        {
            Id = entity.Id,
            LookupTypeId = entity.LookupTypeId,
            Key = entity.Key,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            IsDefault = entity.IsDefault,
            IsSystem = entity.IsSystem,
            IsActive = entity.IsActive
        };
    }
}
