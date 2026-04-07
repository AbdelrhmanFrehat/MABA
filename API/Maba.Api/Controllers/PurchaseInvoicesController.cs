using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/purchase-invoices")]
[Authorize]
public class PurchaseInvoicesController : ControllerBase
{
    /// <summary>
    /// Purchase invoices module is not yet implemented.
    /// Returns an empty collection so the payment form can load without errors.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<object>> GetPurchaseInvoices()
    {
        return Ok(Array.Empty<object>());
    }
}
