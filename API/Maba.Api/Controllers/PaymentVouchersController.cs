using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/payment-vouchers")]
[Authorize]
public class PaymentVouchersController : ControllerBase
{
    /// <summary>
    /// Temporary compatibility endpoint for the admin payment vouchers page.
    /// The frontend route exists, but the full backend payments module is not implemented yet.
    /// Return an empty collection instead of 404 so the page can load safely.
    /// </summary>
    [HttpGet]
    public ActionResult<IEnumerable<object>> GetPaymentVouchers()
    {
        return Ok(Array.Empty<object>());
    }
}
