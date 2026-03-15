using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Projects;

namespace Maba.Application.Features.Projects.Commands;

public class UpdateProjectRequestCommandHandler : IRequestHandler<UpdateProjectRequestCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProjectRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProjectRequestCommand request, CancellationToken cancellationToken)
    {
        var projectRequest = await _context.Set<ProjectRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (projectRequest == null) return false;

        if (request.Status.HasValue)
        {
            projectRequest.Status = request.Status.Value;
        }

        if (request.AdminNotes != null)
        {
            projectRequest.AdminNotes = request.AdminNotes;
        }

        projectRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
