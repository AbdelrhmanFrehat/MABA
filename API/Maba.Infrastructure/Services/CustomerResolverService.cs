using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Crm;
using Maba.Domain.Lookups;
using Maba.Domain.Numbering;
using Maba.Infrastructure.Data;

namespace Maba.Infrastructure.Services;

/// <summary>
/// Finds or creates a Customer from a service request submission.
/// Uses a "find-or-create" pattern to prevent duplicates.
/// </summary>
public class CustomerResolverService : ICustomerResolverService
{
    private readonly ApplicationDbContext _context;
    private readonly IDocumentNumberService _documentNumberService;
    private readonly ILogger<CustomerResolverService> _logger;

    public CustomerResolverService(
        ApplicationDbContext context,
        IDocumentNumberService documentNumberService,
        ILogger<CustomerResolverService> logger)
    {
        _context = context;
        _documentNumberService = documentNumberService;
        _logger = logger;
    }

    public async Task<Guid> ResolveAsync(
        Guid? userId,
        string name,
        string? email,
        string? phone,
        CancellationToken cancellationToken = default)
    {
        Customer? customer = null;

        // 1. Look up by UserId (most specific match — same website account)
        if (userId.HasValue)
        {
            customer = await _context.Set<Customer>()
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.IsActive, cancellationToken);
        }

        // 2. Look up by email (covers cases where account was linked manually or a previous anonymous request was matched)
        if (customer is null && !string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            customer = await _context.Set<Customer>()
                .FirstOrDefaultAsync(
                    c => c.Email != null && c.Email.ToLower() == normalizedEmail && c.IsActive,
                    cancellationToken);

            // If found by email and we now know the userId, link them
            if (customer is not null && userId.HasValue && customer.UserId is null)
            {
                customer.UserId = userId.Value;
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Linked website user {UserId} to existing customer {CustomerId} via email match",
                    userId.Value, customer.Id);
            }
        }

        if (customer is not null)
        {
            return customer.Id;
        }

        // 3. Create a new customer
        var customerTypeId = await ResolveCustomerTypeIdAsync(cancellationToken);

        var safeName = string.IsNullOrWhiteSpace(name) ? "Website Customer" : name.Trim();

        string customerCode;
        try
        {
            customerCode = await _documentNumberService.GenerateNextAsync("Customer", cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            // Sequence not seeded yet — generate a fallback code and seed on the fly
            _logger.LogWarning("Customer document sequence not found. Seeding it now.");
            customerCode = await SeedCustomerSequenceAndGenerateAsync(cancellationToken);
        }

        var newCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Code = customerCode,
            NameEn = safeName,
            NameAr = safeName,
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            CustomerTypeId = customerTypeId,
            UserId = userId,
            Currency = "ILS",
            CreditLimit = 0,
            CurrentBalance = 0,
            IsActive = true
        };

        _context.Set<Customer>().Add(newCustomer);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created new customer {CustomerId} (Code: {Code}) for website submission. UserId={UserId}, Email={Email}",
            newCustomer.Id, newCustomer.Code, userId, email);

        return newCustomer.Id;
    }

    private async Task<Guid> SeedDefaultCustomerTypeAsync(CancellationToken cancellationToken)
    {
        // Find or create the LookupType for CustomerType
        var lookupType = await _context.Set<LookupType>()
            .FirstOrDefaultAsync(t => t.Key == "CustomerType", cancellationToken);

        if (lookupType is null)
        {
            lookupType = new LookupType
            {
                Id = Guid.NewGuid(),
                Key = "CustomerType",
                NameEn = "Customer Type",
                NameAr = "نوع العميل",
                IsSystem = true,
                IsActive = true
            };
            _context.Set<LookupType>().Add(lookupType);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var value = new LookupValue
        {
            Id = Guid.NewGuid(),
            LookupTypeId = lookupType.Id,
            Key = "Retail",
            NameEn = "Retail",
            NameAr = "تجزئة",
            SortOrder = 1,
            IsDefault = true,
            IsActive = true
        };
        _context.Set<LookupValue>().Add(value);
        await _context.SaveChangesAsync(cancellationToken);
        return value.Id;
    }

    private async Task<string> SeedCustomerSequenceAndGenerateAsync(CancellationToken cancellationToken)
    {
        var existingCount = await _context.Set<Customer>().CountAsync(cancellationToken);
        var nextNumber = existingCount + 1;
        var currentYear = DateTime.UtcNow.Year;

        var sequence = new DocumentSequence
        {
            Id = Guid.NewGuid(),
            DocumentType = "Customer",
            Prefix = "CUST",
            Separator = "-",
            IncludeYear = true,
            CurrentYear = currentYear,
            LastNumber = nextNumber,
            PadLength = 4,
            IsActive = true
        };
        _context.Set<DocumentSequence>().Add(sequence);
        await _context.SaveChangesAsync(cancellationToken);

        return $"CUST-{currentYear}-{nextNumber.ToString().PadLeft(4, '0')}";
    }

    private async Task<Guid> ResolveCustomerTypeIdAsync(CancellationToken cancellationToken)
    {
        // Prefer "Retail" from the CustomerType lookup; fall back to any available type
        var typeId = await _context.Set<LookupValue>()
            .Where(v => v.LookupType.Key == "CustomerType" && v.Key == "Retail")
            .Select(v => (Guid?)v.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (typeId.HasValue)
        {
            return typeId.Value;
        }

        // Fall back to first available CustomerType
        typeId = await _context.Set<LookupValue>()
            .Where(v => v.LookupType.Key == "CustomerType")
            .Select(v => (Guid?)v.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (typeId.HasValue)
        {
            return typeId.Value;
        }

        // If no lookup type exists at all (fresh DB before seeding), seed it now
        _logger.LogWarning("No CustomerType lookup values found. Seeding a default 'Retail' type on demand.");
        return await SeedDefaultCustomerTypeAsync(cancellationToken);
    }
}
