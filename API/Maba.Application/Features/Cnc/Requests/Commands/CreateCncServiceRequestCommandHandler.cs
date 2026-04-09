using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Domain.Cnc;

namespace Maba.Application.Features.Cnc.Requests.Commands;

public class CreateCncServiceRequestCommandHandler : IRequestHandler<CreateCncServiceRequestCommand, CreateCncServiceRequestResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ICustomerResolverService _customerResolver;
    private readonly ILogger<CreateCncServiceRequestCommandHandler> _logger;

    public CreateCncServiceRequestCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ICustomerResolverService customerResolver,
        ILogger<CreateCncServiceRequestCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _customerResolver = customerResolver;
        _logger = logger;
    }

    public async Task<CreateCncServiceRequestResultDto> Handle(CreateCncServiceRequestCommand request, CancellationToken cancellationToken)
    {
        if (request.ServiceMode == "routing")
        {
            if (!request.MaterialId.HasValue)
            {
                throw new ArgumentException("Material is required for CNC routing requests.");
            }

            var material = await _context.Set<CncMaterial>()
                .FirstOrDefaultAsync(m => m.Id == request.MaterialId.Value && m.IsActive, cancellationToken);

            if (material == null)
            {
                throw new ArgumentException("Invalid or unavailable material selected.");
            }

            if (request.ThicknessMm.HasValue)
            {
                if (request.ThicknessMm.Value < material.MinThicknessMm || request.ThicknessMm.Value > material.MaxThicknessMm)
                {
                    throw new ArgumentException($"Thickness must be between {material.MinThicknessMm}mm and {material.MaxThicknessMm}mm for this material.");
                }
            }

            const decimal maxDimension = 400m;
            if (request.WidthMm.HasValue && (request.WidthMm.Value <= 0 || request.WidthMm.Value > maxDimension))
            {
                throw new ArgumentException($"Width must be between 0 and {maxDimension} mm.");
            }
            if (request.HeightMm.HasValue && (request.HeightMm.Value <= 0 || request.HeightMm.Value > maxDimension))
            {
                throw new ArgumentException($"Height must be between 0 and {maxDimension} mm.");
            }
        }

        var customerId = await _customerResolver.ResolveAsync(
            request.UserId,
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone,
            cancellationToken);

        var referenceNumber = await GenerateReferenceNumberAsync(cancellationToken);

        var serviceRequest = new CncServiceRequest
        {
            Id = Guid.NewGuid(),
            ReferenceNumber = referenceNumber,
            ServiceMode = request.ServiceMode,
            MaterialId = request.MaterialId,
            PcbMaterial = request.PcbMaterial,
            PcbThickness = request.PcbThickness,
            PcbSide = request.PcbSide,
            PcbOperation = request.PcbOperation,
            OperationType = request.OperationType,
            WidthMm = request.WidthMm,
            HeightMm = request.HeightMm,
            ThicknessMm = request.ThicknessMm,
            Quantity = request.Quantity,
            DepthMode = request.DepthMode,
            DepthMm = request.DepthMm,
            DesignSourceType = request.DesignSourceType,
            FilePath = request.FilePath,
            FileName = request.FileName,
            DesignNotes = request.DesignNotes,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ProjectDescription = request.ProjectDescription,
            UserId = request.UserId,
            CustomerId = customerId,
            Status = CncServiceRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<CncServiceRequest>().Add(serviceRequest);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendRequestConfirmationAsync(serviceRequest.CustomerEmail, serviceRequest.CustomerName, referenceNumber, "CNC Service Request", null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email for CNC request {ReferenceNumber}", referenceNumber);
        }

        return new CreateCncServiceRequestResultDto
        {
            Id = serviceRequest.Id,
            ReferenceNumber = referenceNumber,
            Message = "CNC service request submitted successfully. We will contact you shortly."
        };
    }

    private async Task<string> GenerateReferenceNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var datePrefix = today.ToString("yyyyMMdd");
        
        var todayStart = today.Date;
        var todayEnd = todayStart.AddDays(1);

        var countToday = await _context.Set<CncServiceRequest>()
            .CountAsync(r => r.CreatedAt >= todayStart && r.CreatedAt < todayEnd, cancellationToken);

        var sequence = (countToday + 1).ToString("D3");
        return $"CNC-{datePrefix}-{sequence}";
    }
}
