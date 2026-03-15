using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Media.Commands;
using Maba.Domain.Media;

namespace Maba.Application.Features.Media.Handlers;

public class DeleteMediaCommandHandler : IRequestHandler<DeleteMediaCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public DeleteMediaCommandHandler(
        IApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<Unit> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        var mediaAsset = await _context.Set<MediaAsset>()
            .Include(m => m.MediaType)
            .Include(m => m.EntityMediaLinks)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (mediaAsset == null)
        {
            throw new KeyNotFoundException("Media asset not found");
        }

        // Delete file from disk
        try
        {
            var webRootPath = _configuration["FileStorage:WebRootPath"] ?? "wwwroot";
            var filePath = Path.Combine(webRootPath, mediaAsset.FileUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Log error but continue with database deletion
        }

        // Remove entity media links
        var links = mediaAsset.EntityMediaLinks.ToList();
        foreach (var link in links)
        {
            _context.Set<EntityMediaLink>().Remove(link);
        }

        // Remove media asset
        _context.Set<MediaAsset>().Remove(mediaAsset);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

