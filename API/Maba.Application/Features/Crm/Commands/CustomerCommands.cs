using MediatR;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Application.Features.Crm.DTOs;
using Maba.Domain.Crm;

namespace Maba.Application.Features.Crm.Commands;

public class CreateCustomerCommand : IRequest<CustomerDto>
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }
    public Guid CustomerTypeId { get; set; }
    public Guid? UserId { get; set; }
    public string? TaxNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public string Currency { get; set; } = "ILS";
    public bool IsActive { get; set; } = true;
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberService _documentNumberService;

    public CreateCustomerCommandHandler(IApplicationDbContext context, IDocumentNumberService documentNumberService)
    {
        _context = context;
        _documentNumberService = documentNumberService;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            Code = await _documentNumberService.GenerateNextAsync("Customer", cancellationToken),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Email = request.Email,
            Phone = request.Phone,
            Phone2 = request.Phone2,
            CustomerTypeId = request.CustomerTypeId,
            UserId = request.UserId,
            TaxNumber = request.TaxNumber,
            CreditLimit = request.CreditLimit,
            CurrentBalance = 0,
            Currency = request.Currency,
            IsActive = request.IsActive
        };

        _context.Set<Customer>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = entity.Id,
            Code = entity.Code,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
            Email = entity.Email,
            Phone = entity.Phone,
            CustomerTypeId = entity.CustomerTypeId,
            UserId = entity.UserId,
            CreditLimit = entity.CreditLimit,
            CurrentBalance = entity.CurrentBalance,
            Currency = entity.Currency,
            IsActive = entity.IsActive
        };
    }
}

public class UpdateCustomerCommand : IRequest<CustomerDto>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }
    public Guid CustomerTypeId { get; set; }
    public Guid? UserId { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<Customer>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Customer not found.");

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.Phone2 = request.Phone2;
        entity.CustomerTypeId = request.CustomerTypeId;
        entity.UserId = request.UserId;
        entity.CreditLimit = request.CreditLimit;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = entity.Id,
            Code = entity.Code,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
            Email = entity.Email,
            Phone = entity.Phone,
            CustomerTypeId = entity.CustomerTypeId,
            UserId = entity.UserId,
            CreditLimit = entity.CreditLimit,
            CurrentBalance = entity.CurrentBalance,
            Currency = entity.Currency,
            IsActive = entity.IsActive
        };
    }
}

public class CreateSupplierCommand : IRequest<SupplierDto>
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }
    public Guid? SupplierTypeId { get; set; }
    public decimal CreditLimit { get; set; }
    public string Currency { get; set; } = "ILS";
    public int PaymentTermDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDocumentNumberService _documentNumberService;

    public CreateSupplierCommandHandler(IApplicationDbContext context, IDocumentNumberService documentNumberService)
    {
        _context = context;
        _documentNumberService = documentNumberService;
    }

    public async Task<SupplierDto> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var entity = new Supplier
        {
            Id = Guid.NewGuid(),
            Code = await _documentNumberService.GenerateNextAsync("Supplier", cancellationToken),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Email = request.Email,
            Phone = request.Phone,
            Phone2 = request.Phone2,
            SupplierTypeId = request.SupplierTypeId,
            CreditLimit = request.CreditLimit,
            CurrentBalance = 0,
            Currency = request.Currency,
            PaymentTermDays = request.PaymentTermDays,
            IsActive = request.IsActive
        };

        _context.Set<Supplier>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new SupplierDto
        {
            Id = entity.Id,
            Code = entity.Code,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
            Email = entity.Email,
            Phone = entity.Phone,
            SupplierTypeId = entity.SupplierTypeId,
            CreditLimit = entity.CreditLimit,
            CurrentBalance = entity.CurrentBalance,
            Currency = entity.Currency,
            PaymentTermDays = entity.PaymentTermDays,
            IsActive = entity.IsActive
        };
    }
}

public class UpdateSupplierCommand : IRequest<SupplierDto>
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }
    public Guid? SupplierTypeId { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, SupplierDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSupplierCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierDto> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<Supplier>().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Supplier not found.");

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.Phone2 = request.Phone2;
        entity.SupplierTypeId = request.SupplierTypeId;
        entity.CreditLimit = request.CreditLimit;
        entity.PaymentTermDays = request.PaymentTermDays;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new SupplierDto
        {
            Id = entity.Id,
            Code = entity.Code,
            NameEn = entity.NameEn,
            NameAr = entity.NameAr,
            Email = entity.Email,
            Phone = entity.Phone,
            SupplierTypeId = entity.SupplierTypeId,
            CreditLimit = entity.CreditLimit,
            CurrentBalance = entity.CurrentBalance,
            Currency = entity.Currency,
            PaymentTermDays = entity.PaymentTermDays,
            IsActive = entity.IsActive
        };
    }
}
