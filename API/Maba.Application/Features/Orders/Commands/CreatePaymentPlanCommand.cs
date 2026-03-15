using MediatR;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Features.Orders.Commands;

public class CreatePaymentPlanCommand : IRequest<PaymentPlanDto>
{
    public Guid OrderId { get; set; }
    public decimal DownPayment { get; set; }
    public int InstallmentsCount { get; set; }
    public string InstallmentFrequency { get; set; } = "Monthly";
    public decimal InterestRate { get; set; }
}

