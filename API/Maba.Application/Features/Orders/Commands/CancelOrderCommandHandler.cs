using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Commands;
using Maba.Application.Features.Orders.DTOs;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Orders.Handlers;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IAuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CancelOrderCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        IAuditService auditService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _auditService = auditService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<OrderDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<Order>()
            .Include(o => o.OrderStatus)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        // Check if order can be cancelled
        var statusKey = order.OrderStatus.Key;
        if (statusKey == "Cancelled")
        {
            throw new InvalidOperationException("Order is already cancelled.");
        }

        if (statusKey == "Delivered")
        {
            throw new InvalidOperationException("Cannot cancel a delivered order.");
        }

        // Get cancelled status
        var cancelledStatus = await _context.Set<OrderStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Cancelled", cancellationToken);

        if (cancelledStatus == null)
        {
            throw new KeyNotFoundException("Cancelled order status not found.");
        }

        // Update order status
        order.OrderStatusId = cancelledStatus.Id;
        if (!string.IsNullOrEmpty(request.Reason))
        {
            order.Notes = string.IsNullOrEmpty(order.Notes) 
                ? $"Cancelled: {request.Reason}" 
                : $"{order.Notes}\nCancelled: {request.Reason}";
        }
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Use UpdateOrderStatusCommandHandler to handle inventory restoration
        var updateStatusCommand = new UpdateOrderStatusCommand
        {
            OrderId = request.OrderId,
            OrderStatusId = cancelledStatus.Id
        };

        var updateHandler = new UpdateOrderStatusCommandHandler(_context, _emailService, _configuration, _auditService, _httpContextAccessor);
        return await updateHandler.Handle(updateStatusCommand, cancellationToken);
    }
}

