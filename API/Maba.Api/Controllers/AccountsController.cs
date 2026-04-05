using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    /// <summary>
    /// Temporary compatibility endpoint for the admin chart of accounts page.
    /// The frontend route exists, but the full backend accounting module is not implemented yet.
    /// Return an empty collection instead of 404 so the page can load safely.
    /// </summary>
    [HttpGet("tree")]
    public ActionResult<IEnumerable<object>> GetAccountTree()
    {
        return Ok(Array.Empty<object>());
    }
}
