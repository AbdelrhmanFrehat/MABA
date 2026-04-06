using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/purchase-orders")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    /// <summary>
    /// Temporary compatibility endpoint for the admin purchase orders page.
    /// The frontend route exists, but the full backend purchasing module is not implemented yet.
    /// Return an empty collection instead of 404 so the page can load safely.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<object>> GetPurchaseOrders()
    {
        return Ok(Array.Empty<object>());
    }
}
