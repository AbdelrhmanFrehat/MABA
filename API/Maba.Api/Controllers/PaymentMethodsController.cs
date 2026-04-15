using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Orders;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    public PaymentMethodsController(IApplicationDbContext context) { _context = context; }

    public class PaymentMethodDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<List<PaymentMethodDto>>> GetAll()
    {
        var list = await _context.Set<PaymentMethod>()
            .OrderBy(m => m.NameEn)
            .Select(m => new PaymentMethodDto { Id = m.Id, Key = m.Key, NameEn = m.NameEn, NameAr = m.NameAr })
            .ToListAsync();
        return Ok(list);
    }
}
