using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/price-lists")]
[Authorize]
public class PriceListsController : ControllerBase
{
    /// <summary>
    /// Temporary compatibility endpoint for the admin price lists page.
    /// The frontend route exists, but the full backend pricing module is not implemented yet.
    /// Return an empty collection instead of 404 so the page can load safely.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<object>> GetPriceLists()
    {
        return Ok(Array.Empty<object>());
    }
}
