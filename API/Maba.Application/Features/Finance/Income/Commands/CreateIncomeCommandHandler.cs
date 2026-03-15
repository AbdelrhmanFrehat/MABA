using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Finance.Income.Commands;
using Maba.Application.Features.Finance.DTOs;
using Maba.Domain.Finance;
using IncomeEntity = Maba.Domain.Finance.Income;

namespace Maba.Application.Features.Finance.Income.Handlers;

public class CreateIncomeCommandHandler : IRequestHandler<CreateIncomeCommand, IncomeDto>
{
    private readonly IApplicationDbContext _context;

    public CreateIncomeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IncomeDto> Handle(CreateIncomeCommand request, CancellationToken cancellationToken)
    {
        var incomeSource = await _context.Set<IncomeSource>()
            .FirstOrDefaultAsync(isrc => isrc.Id == request.IncomeSourceId, cancellationToken);

        if (incomeSource == null)
        {
            throw new KeyNotFoundException("Income source not found");
        }

        var user = await _context.Set<Domain.Users.User>()
            .FirstOrDefaultAsync(u => u.Id == request.EnteredByUserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        var income = new IncomeEntity
        {
            Id = Guid.NewGuid(),
            IncomeSourceId = request.IncomeSourceId,
            RefId = request.RefId,
            Amount = request.Amount,
            Currency = request.Currency,
            ReceivedAt = request.ReceivedAt,
            EnteredByUserId = request.EnteredByUserId
        };

        _context.Set<IncomeEntity>().Add(income);
        await _context.SaveChangesAsync(cancellationToken);

        return new IncomeDto
        {
            Id = income.Id,
            IncomeSourceId = income.IncomeSourceId,
            IncomeSourceKey = incomeSource.Key,
            RefId = income.RefId,
            Amount = income.Amount,
            Currency = income.Currency,
            ReceivedAt = income.ReceivedAt,
            EnteredByUserId = income.EnteredByUserId,
            EnteredByUserFullName = user.FullName,
            CreatedAt = income.CreatedAt,
            UpdatedAt = income.UpdatedAt
        };
    }
}

