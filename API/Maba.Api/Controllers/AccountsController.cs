using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Accounting;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IApplicationDbContext _context;

    public AccountsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccounts(CancellationToken cancellationToken)
    {
        var accounts = await _context.Set<Account>()
            .Include(x => x.AccountType)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return Ok(accounts.Select(ToDto));
    }

    [HttpGet("tree")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAccountTree(CancellationToken cancellationToken)
    {
        var accounts = await _context.Set<Account>()
            .Include(x => x.AccountType)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var dtoMap = accounts.ToDictionary(a => a.Id, a => ToDto(a));

        // attach children
        foreach (var account in accounts)
        {
            if (account.ParentId.HasValue && dtoMap.TryGetValue(account.ParentId.Value, out var parent))
            {
                parent.Children ??= new List<AccountDto>();
                parent.Children.Add(dtoMap[account.Id]);
            }
        }

        var roots = dtoMap.Values.Where(d => d.ParentAccountId == null).ToList();
        return Ok(roots);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccountDto>> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        var account = await _context.Set<Account>()
            .Include(x => x.AccountType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (account == null) return NotFound();
        return Ok(ToDto(account));
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var accountType = await _context.Set<AccountType>()
            .FirstOrDefaultAsync(x => x.Id == request.AccountTypeLookupId, cancellationToken);
        if (accountType == null)
            return BadRequest("Account type not found.");

        int level = 1;
        if (request.ParentAccountId.HasValue)
        {
            var parent = await _context.Set<Account>()
                .FirstOrDefaultAsync(x => x.Id == request.ParentAccountId.Value, cancellationToken);
            if (parent == null) return BadRequest("Parent account not found.");
            level = parent.Level + 1;
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Code = request.AccountNumber,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            AccountTypeId = request.AccountTypeLookupId,
            ParentId = request.ParentAccountId,
            Level = level,
            IsPostable = request.IsPostable ?? true,
            IsActive = true,
            Description = request.Description
        };

        _context.Set<Account>().Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        account.AccountType = accountType;
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, ToDto(account));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AccountDto>> UpdateAccount(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var account = await _context.Set<Account>()
            .Include(x => x.AccountType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (account == null) return NotFound();
        if (account.IsSystem)
            return BadRequest("System accounts cannot be modified.");

        account.NameEn = request.NameEn;
        account.NameAr = request.NameAr;
        account.IsActive = request.IsActive;
        account.IsPostable = request.IsPostable;
        account.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(account));
    }

    private static AccountDto ToDto(Account a) => new()
    {
        Id = a.Id,
        AccountNumber = a.Code,
        NameEn = a.NameEn,
        NameAr = a.NameAr,
        AccountTypeLookupId = a.AccountTypeId,
        AccountTypeName = a.AccountType?.NameEn,
        ParentAccountId = a.ParentId,
        IsActive = a.IsActive,
        IsSystem = a.IsSystem,
        IsPostable = a.IsPostable,
        Description = a.Description,
        Balance = a.CurrentBalance,
        NormalBalance = a.AccountType?.NormalBalance ?? string.Empty,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };

    public class AccountDto
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public Guid AccountTypeLookupId { get; set; }
        public string? AccountTypeName { get; set; }
        public Guid? ParentAccountId { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystem { get; set; }
        public bool IsPostable { get; set; }
        public string? Description { get; set; }
        public decimal Balance { get; set; }
        public string NormalBalance { get; set; } = string.Empty;
        public List<AccountDto>? Children { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateAccountRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public Guid AccountTypeLookupId { get; set; }
        public Guid? ParentAccountId { get; set; }
        public bool? IsPostable { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateAccountRequest
    {
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsPostable { get; set; }
        public string? Description { get; set; }
    }
}
