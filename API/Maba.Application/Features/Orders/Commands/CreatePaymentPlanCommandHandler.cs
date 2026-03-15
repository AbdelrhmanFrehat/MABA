using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Orders.Commands;
using Maba.Application.Features.Orders.DTOs;
using Maba.Domain.Orders;

namespace Maba.Application.Features.Orders.Handlers;

public class CreatePaymentPlanCommandHandler : IRequestHandler<CreatePaymentPlanCommand, PaymentPlanDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePaymentPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentPlanDto> Handle(CreatePaymentPlanCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Set<Order>()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found");
        }

        var remainingAmount = order.Total - request.DownPayment;
        var installmentAmount = remainingAmount / request.InstallmentsCount;

        var paymentPlan = new PaymentPlan
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            DownPayment = request.DownPayment,
            InstallmentsCount = request.InstallmentsCount,
            InstallmentFrequency = request.InstallmentFrequency,
            InterestRate = request.InterestRate
        };

        _context.Set<PaymentPlan>().Add(paymentPlan);
        await _context.SaveChangesAsync(cancellationToken);

        // Create installments
        var pendingStatus = await _context.Set<InstallmentStatus>()
            .FirstOrDefaultAsync(s => s.Key == "Pending", cancellationToken);

        if (pendingStatus == null)
        {
            throw new KeyNotFoundException("Pending installment status not found");
        }

        var installments = new List<Installment>();
        var dueDate = DateTime.UtcNow;

        for (int i = 1; i <= request.InstallmentsCount; i++)
        {
            if (request.InstallmentFrequency == "Monthly")
            {
                dueDate = dueDate.AddMonths(1);
            }
            else if (request.InstallmentFrequency == "Weekly")
            {
                dueDate = dueDate.AddDays(7);
            }

            installments.Add(new Installment
            {
                Id = Guid.NewGuid(),
                PaymentPlanId = paymentPlan.Id,
                Seq = i,
                DueDate = dueDate,
                Amount = installmentAmount,
                InstallmentStatusId = pendingStatus.Id
            });
        }

        _context.Set<Installment>().AddRange(installments);
        await _context.SaveChangesAsync(cancellationToken);

        // Load payment plan with installments
        var planWithInstallments = await _context.Set<PaymentPlan>()
            .Include(pp => pp.Installments)
            .ThenInclude(i => i.InstallmentStatus)
            .FirstOrDefaultAsync(pp => pp.Id == paymentPlan.Id, cancellationToken);

        return new PaymentPlanDto
        {
            Id = planWithInstallments!.Id,
            OrderId = planWithInstallments.OrderId,
            DownPayment = planWithInstallments.DownPayment,
            InstallmentsCount = planWithInstallments.InstallmentsCount,
            InstallmentFrequency = planWithInstallments.InstallmentFrequency,
            InterestRate = planWithInstallments.InterestRate,
            Installments = planWithInstallments.Installments.Select(i => new InstallmentDto
            {
                Id = i.Id,
                PaymentPlanId = i.PaymentPlanId,
                Seq = i.Seq,
                DueDate = i.DueDate,
                Amount = i.Amount,
                InstallmentStatusId = i.InstallmentStatusId,
                InstallmentStatusKey = i.InstallmentStatus.Key,
                PaidAt = i.PaidAt,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList(),
            CreatedAt = planWithInstallments.CreatedAt,
            UpdatedAt = planWithInstallments.UpdatedAt
        };
    }
}

