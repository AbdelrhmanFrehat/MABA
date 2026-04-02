using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Maba.Application.Features.Cnc.DTOs;
using Maba.Application.Features.Cnc.Requests.Queries;
using Maba.Domain.Cnc;

namespace Maba.Api.Controllers;

[ApiController]
[Route("api/v1/cnc/requests")]
public class CncServiceRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CncServiceRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Admin list: CNC service requests with filters and paging.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<CncServiceRequestsListResult>> GetRequests(
        [FromQuery] CncServiceRequestStatus? status = null,
        [FromQuery] string? serviceMode = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 25)
    {
        var query = new GetAllCncServiceRequestsQuery
        {
            Status = status,
            ServiceMode = string.IsNullOrWhiteSpace(serviceMode) ? null : serviceMode.Trim(),
            SearchTerm = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            CreatedFromUtc = createdFrom,
            CreatedToUtc = createdTo,
            Skip = skip,
            Take = take
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get one request by id (admin).
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<CncServiceRequestDto>> GetById(Guid id)
    {
        var q = new GetCncServiceRequestByIdQuery { Id = id };
        var dto = await _mediator.Send(q);
        if (dto == null)
        {
            return NotFound();
        }

        return Ok(dto);
    }
}
