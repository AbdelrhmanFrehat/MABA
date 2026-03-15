using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Commands;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Orders.Handlers;

public class AddOrderNoteCommandHandler : IRequestHandler<AddOrderNoteCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public AddOrderNoteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(AddOrderNoteCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var newNote = $"[{timestamp}] {request.Note}";

        order.Notes = string.IsNullOrEmpty(order.Notes)
            ? newNote
            : $"{order.Notes}\n{newNote}";
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

