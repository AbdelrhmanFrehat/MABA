using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Laser.DTOs;
using Maba.Domain.Laser;

namespace Maba.Application.Features.Laser.Requests.Commands;

public class CreateLaserServiceRequestCommandHandler : IRequestHandler<CreateLaserServiceRequestCommand, CreateLaserServiceRequestResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateLaserServiceRequestCommandHandler> _logger;

    public CreateLaserServiceRequestCommandHandler(IApplicationDbContext context, IEmailService emailService, ILogger<CreateLaserServiceRequestCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<CreateLaserServiceRequestResultDto> Handle(CreateLaserServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var material = await _context.Set<LaserMaterial>()
            .FirstOrDefaultAsync(m => m.Id == request.MaterialId && m.IsActive && !m.IsMetal, cancellationToken);

        if (material == null)
        {
            throw new ArgumentException("Invalid or unavailable material selected.");
        }

        var operationMode = request.OperationMode?.Trim().ToLowerInvariant() ?? "engrave";
        if (operationMode != "cut" && operationMode != "engrave")
        {
            operationMode = "engrave";
        }

        if (material.Type != "both" && material.Type != operationMode)
        {
            throw new ArgumentException($"This material does not support '{operationMode}' operation.");
        }

        // Validate dimensions (max 40cm x 40cm)
        const decimal maxDimension = 40m;
        var widthCm = request.WidthCm;
        var heightCm = request.HeightCm;
        
        if (widthCm.HasValue && (widthCm.Value <= 0 || widthCm.Value > maxDimension))
        {
            throw new ArgumentException($"Width must be between 0 and {maxDimension} cm.");
        }
        if (heightCm.HasValue && (heightCm.Value <= 0 || heightCm.Value > maxDimension))
        {
            throw new ArgumentException($"Height must be between 0 and {maxDimension} cm.");
        }

        var referenceNumber = await GenerateReferenceNumberAsync(cancellationToken);

        var serviceRequest = new LaserServiceRequest
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = referenceNumber,
            MaterialId = request.MaterialId,
            OperationMode = operationMode,
            ImagePath = request.ImagePath,
            ImageFileName = request.ImageFileName,
            WidthCm = widthCm,
            HeightCm = heightCm,
            CustomerName = request.CustomerName?.Trim(),
            CustomerEmail = request.CustomerEmail?.Trim(),
            CustomerPhone = request.CustomerPhone?.Trim(),
            CustomerNotes = request.CustomerNotes?.Trim(),
            Status = LaserServiceRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<LaserServiceRequest>().Add(serviceRequest);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendRequestConfirmationAsync(serviceRequest.CustomerEmail, serviceRequest.CustomerName, referenceNumber, "Laser Service Request", null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for Laser request {ReferenceNumber}", referenceNumber);
        }

        return new CreateLaserServiceRequestResultDto
        {
            Id = serviceRequest.Id,
            ReferenceNumber = serviceRequest.ReferenceNumber,
            Message = "Your laser service request has been submitted successfully. Our team will review it and contact you shortly."
        };
    }

    private async Task<string> GenerateReferenceNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var datePrefix = today.ToString("yyyyMMdd");
        
        var todayStart = today.Date;
        var todayEnd = todayStart.AddDays(1);

        var countToday = await _context.Set<LaserServiceRequest>()
            .CountAsync(r => r.CreatedAt >= todayStart && r.CreatedAt < todayEnd, cancellationToken);

        var sequence = (countToday + 1).ToString("D3");
        return $"LSR-{datePrefix}-{sequence}";
    }
}
