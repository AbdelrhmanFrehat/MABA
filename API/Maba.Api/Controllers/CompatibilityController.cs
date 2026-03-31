using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maba.Api.Controllers;

/// <summary>
/// Compatibility endpoints for third-party runtime probes hitting /api/_.../check.
/// These requests are not part of MABA API contract but can generate noisy 404s in browser consoles.
/// </summary>
[ApiController]
[Route("api")]
public class CompatibilityController : ControllerBase
{
    [HttpGet("{token}/check")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult ThirdPartyCheck(string token)
    {
        // Only acknowledge probe-like tokens that start with "_".
        // Any other unknown /api/{token}/check path should remain not found.
        if (!string.IsNullOrWhiteSpace(token) && token.StartsWith("_", StringComparison.Ordinal))
        {
            return NoContent();
        }

        return NotFound();
    }
}
