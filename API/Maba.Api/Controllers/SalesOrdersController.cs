using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/sales-orders")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    /// <summary>
    /// Temporary compatibility endpoint for the admin sales orders page.
    /// The frontend route exists, but the full backend sales module is not implemented yet.
    /// Return an empty collection instead of 404 so the page can load safely.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<object>> GetSalesOrders()
    {
        return Ok(Array.Empty<object>());
    }
}
