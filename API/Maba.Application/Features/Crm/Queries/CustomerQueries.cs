using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Crm.DTOs;
using Maba.Domain.Crm;

namespace Maba.Application.Features.Crm.Queries;

public class GetCustomersQuery : IRequest<List<CustomerDto>>
{
}

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, List<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<Customer>()
            .OrderBy(x => x.Code)
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Email = x.Email,
                Phone = x.Phone,
                CustomerTypeId = x.CustomerTypeId,
                CreditLimit = x.CreditLimit,
                CurrentBalance = x.CurrentBalance,
                Currency = x.Currency,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}

public class GetCustomerByIdQuery : IRequest<CustomerDto?>
{
    public Guid Id { get; set; }
}

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<Customer>()
            .Where(x => x.Id == request.Id)
            .Select(x => new CustomerDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Email = x.Email,
                Phone = x.Phone,
                CustomerTypeId = x.CustomerTypeId,
                CreditLimit = x.CreditLimit,
                CurrentBalance = x.CurrentBalance,
                Currency = x.Currency,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class GetSuppliersQuery : IRequest<List<SupplierDto>>
{
}

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, List<SupplierDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSuppliersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<Supplier>()
            .OrderBy(x => x.Code)
            .Select(x => new SupplierDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Email = x.Email,
                Phone = x.Phone,
                SupplierTypeId = x.SupplierTypeId,
                CreditLimit = x.CreditLimit,
                CurrentBalance = x.CurrentBalance,
                Currency = x.Currency,
                PaymentTermDays = x.PaymentTermDays,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}

public class GetSupplierByIdQuery : IRequest<SupplierDto?>
{
    public Guid Id { get; set; }
}

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSupplierByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierDto?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<Supplier>()
            .Where(x => x.Id == request.Id)
            .Select(x => new SupplierDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Email = x.Email,
                Phone = x.Phone,
                SupplierTypeId = x.SupplierTypeId,
                CreditLimit = x.CreditLimit,
                CurrentBalance = x.CurrentBalance,
                Currency = x.Currency,
                PaymentTermDays = x.PaymentTermDays,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
